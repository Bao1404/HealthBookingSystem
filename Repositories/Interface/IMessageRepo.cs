using BusinessObject.Models;

namespace Repositories.IRepositories
{
    public interface IMessageRepository
    {
        Task<Message?> GetMessageById(int messageId);
        Task<List<Message>> GetMessagesByConversationId(int conversationId);
        Task<Message> SendMessageAsync(int conversationId, int senderId, string content);

        Task CreateMessage(Message message);
        Task UpdateMessage(Message message);
        Task DeleteMessage(int messageId);
    }
}
