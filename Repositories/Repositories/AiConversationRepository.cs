using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class AiConversationRepository : IAiConversationRepository
    {
        private readonly HealthCareSystemContext _context;
        public AiConversationRepository(HealthCareSystemContext context)
        {
            _context = context;
        }
        public async Task CreateConversation(Aiconversation conversation)
        {
            try
            {
                await _context.Aiconversations.AddAsync(conversation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Aiconversation> GetConversationByUserId(int userId)
        {
            try
            {
                return await _context.Aiconversations.FirstOrDefaultAsync(c => c.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task UpdateConversation(Aiconversation conversation)
        {
            try
            {
                _context.Entry<Aiconversation>(conversation).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
