using BusinessObject.Models;

using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;

namespace Repositories.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly HealthCareSystemContext _context;

        public MessageRepository(HealthCareSystemContext context)
        {
            _context = context;
        }

        public async Task<Message?> GetMessageById(int messageId)
        {
            try
            {
                return await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Conversation)
                    .FirstOrDefaultAsync(m => m.MessageId == messageId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Message>> GetMessagesByConversationId(int conversationId)
        {
            try
            {
                return await _context.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateMessage(Message message)
        {
            try
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateMessage(Message message)
        {
            try
            {
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteMessage(int messageId)
        {
            try
            {
                var msg = await _context.Messages.FindAsync(messageId);
                if (msg != null)
                {
                    _context.Messages.Remove(msg);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Message> SendMessageAsync(int conversationId, int senderId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

    }
}
