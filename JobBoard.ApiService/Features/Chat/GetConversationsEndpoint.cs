using FastEndpoints;
using JobBoard.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Chat;

public class GetConversationsEndpoint : EndpointWithoutRequest<List<ConversationDto>>
{
    private readonly JobPortalDbContext _db;
    public GetConversationsEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/messages/conversations");
        Roles("User");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var lastMessageIds = await _db.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.OrderByDescending(m => m.SentAt).Select(m => m.Id).FirstOrDefault())
            .ToListAsync(ct);

        var messagesWithSenders = await _db.Messages
            .Where(m => lastMessageIds.Contains(m.Id))
            .Join(_db.Users,
                m => m.SenderId,
                u => u.Id,
                (m, u) => new { m, SenderNickname = u.Nickname })
            .ToListAsync(ct);

        var otherUserIds = messagesWithSenders
            .Select(x => x.m.SenderId == userId ? x.m.ReceiverId : x.m.SenderId)
            .Distinct()
            .ToList();

        var usersDict = await _db.Users
            .Where(u => otherUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Nickname, ct);

        var result = messagesWithSenders
            .Select(x => {
                var otherUserId = x.m.SenderId == userId ? x.m.ReceiverId : x.m.SenderId;
                var senderName = x.m.SenderId == userId ? "You" : x.SenderNickname;

                return new ConversationDto(
                    otherUserId,
                    usersDict.GetValueOrDefault(otherUserId) ?? "Unknown",
                    x.m.Content,
                    senderName,
                    x.m.SentAt,
                    x.m.ReceiverId == userId && !x.m.IsRead
                );
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        await Send.OkAsync(result, ct);
    }
}

public record ConversationDto(
    Guid OtherUserId,
    string OtherUserName,
    string LastMessage,
    string LastSenderNickname,
    DateTimeOffset LastMessageAt,
    bool HasUnread);