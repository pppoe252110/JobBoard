using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Chat.Models;
using JobBoard.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Chat;

// DTOs
public record SendMessageRequest(Guid ReceiverId, string Content);
public record MessageDto(Guid Id, Guid SenderId, Guid ReceiverId, string Content, DateTimeOffset SentAt, bool IsRead, string SenderNickname);

// 1. Send Message Endpoint
public class SendMessageEndpoint : Endpoint<SendMessageRequest>
{
    private readonly JobPortalDbContext _db;

    public SendMessageEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/messages");
        Roles("User");
    }

    public override async Task HandleAsync(SendMessageRequest req, CancellationToken ct)
    {
        var senderIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(senderIdClaim, out var senderId)) return;

        // Prevent user from sending a message to themselves
        if (req.ReceiverId == senderId)
        {
            AddError("You cannot send a message to yourself.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = req.ReceiverId,
            Content = req.Content,
            SentAt = DateTimeOffset.UtcNow,
            IsRead = false
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}

// 2. Get Conversation Endpoint
public class GetConversationEndpoint : EndpointWithoutRequest<List<MessageDto>>
{
    private readonly JobPortalDbContext _db;

    public GetConversationEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/messages/conversation/{otherUserId}");
        Roles("User");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserIdClaim, out var currentUserId)) return;

        var otherUserId = Route<Guid>("otherUserId");

        var messages = await _db.Messages
            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
            .OrderBy(m => m.SentAt)
            .Join(_db.Users, m => m.SenderId, u => u.Id, (m, u) => new MessageDto(
                m.Id, m.SenderId, m.ReceiverId, m.Content, m.SentAt, m.IsRead, u.Nickname
            ))
            .ToListAsync(ct);

        // Mark unread messages as read
        var unread = await _db.Messages.Where(m => m.ReceiverId == currentUserId && m.SenderId == otherUserId && !m.IsRead).ToListAsync(ct);
        foreach (var msg in unread) msg.IsRead = true;
        if (unread.Any()) await _db.SaveChangesAsync(ct);

        await Send.OkAsync(messages, ct);
    }
}