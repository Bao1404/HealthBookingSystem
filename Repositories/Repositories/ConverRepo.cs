using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;

namespace Repositories.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly HealthCareSystemContext _context;

        public ConversationRepository(HealthCareSystemContext context)
        {
            _context = context;
        }

        public async Task<Conversation?> GetConversationById(int conversationId)
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.PatientUser)
                    .Include(c => c.DoctorUser)
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Conversation>> GetConversationsByDoctorId(int doctorId)
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.PatientUser)
                     .Include(c => c.DoctorUser)
                    .Where(c => c.DoctorUserId == doctorId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Conversation>> GetConversationsByPatientId(int patientId)
      
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.PatientUser)
                     .Include(c => c.DoctorUser)
                    .Where(c => c.PatientUserId == patientId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Conversation?> GetConversationByDoctorAndPatient(int doctorId, int patientId)
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.DoctorUserId == doctorId && c.PatientUserId == patientId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateConversation(Conversation conversation)
        {
            try
            {
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateConversation(Conversation conversation)
        {
            try
            {
                _context.Conversations.Update(conversation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteConversation(int conversationId)
        {
            try
            {
                var convo = await _context.Conversations.FindAsync(conversationId);
                if (convo != null)
                {
                    _context.Conversations.Remove(convo);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Conversation> CreateAsync(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation; // Trả về đối tượng cuộc trò chuyện đã tạo
        }
        public async Task<Conversation> FindConversationByPatientIdAndDoctorId(int patientUserId, int doctorUserId)
        {
            return await _context.Conversations
                .Include(c => c.PatientUser)
                .Include(c => c.DoctorUser)
                .FirstOrDefaultAsync(c => c.PatientUserId == patientUserId && c.DoctorUserId == doctorUserId);
        }
    }
}
