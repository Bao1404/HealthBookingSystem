using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class AiMessageRepository : IAiMessageRepository
    {
        private readonly HealthCareSystemContext _context;
        public AiMessageRepository(HealthCareSystemContext context)
        {
            _context = context;
        }
        public async Task CreateMessage(Aimessage msg)
        {
            try
            {
                _context.Aimessages.Add(msg);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<Aimessage>> GetMessagesByUserId(int userId)
        {
            try
            {
                return await _context.Aimessages.Where(m => m.UserId == userId).OrderBy(m => m.SentAt).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task SaveMessage(Aimessage[] msg)
        {
            try
            {
                _context.Aimessages.AddRange(msg);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task DeleteMessageByConversationId(int conversationId)
        {
            try
            {
                var deleteMessages = await _context.Aimessages.Where(m => m.UserId == conversationId).ToListAsync();
                _context.Aimessages.RemoveRange(deleteMessages);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
