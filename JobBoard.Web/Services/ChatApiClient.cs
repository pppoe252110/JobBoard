namespace JobBoard.Web.Services
{
    public class ChatApiClient(HttpClient httpClient)
    {
        public async Task<List<MessageDto>> GetConversationAsync(Guid otherUserId) =>
                await httpClient.GetFromJsonAsync<List<MessageDto>>($"/messages/conversation/{otherUserId}") ?? new();

        public async Task<bool> SendMessageAsync(Guid receiverId, string content)
        {
            var response = await httpClient.PostAsJsonAsync("/messages", new { ReceiverId = receiverId, Content = content });
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ConversationDto>> GetConversationsAsync() =>
        await httpClient.GetFromJsonAsync<List<ConversationDto>>("/messages/conversations") ?? new();
    }
}
public record ConversationDto(
    Guid OtherUserId,
    string OtherUserName,
    string LastMessage,
    string LastSenderNickname,
    DateTimeOffset LastMessageAt,
    bool HasUnread); 
public record MessageDto(Guid Id, Guid SenderId, Guid ReceiverId, string Content, DateTimeOffset SentAt, bool IsRead, string SenderNickname);
