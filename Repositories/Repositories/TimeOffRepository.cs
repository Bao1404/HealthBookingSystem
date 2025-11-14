using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Repositories
{
    public class TimeOffRepository : ITimeOffRepository
    {
        private readonly HealthCareSystemContext _context;

        public TimeOffRepository(HealthCareSystemContext context)
        {
            _context = context;
        }

        public async Task<List<TimeOff>> GetTimeOffsByDoctorAsync(int doctorId)
        {
            return await _context.TimeOffs
                .Where(t => t.DoctorUserId == doctorId)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<TimeOff?> GetTimeOffByIdAsync(int timeOffId)
        {
            return await _context.TimeOffs
                .FirstOrDefaultAsync(t => t.TimeOffId == timeOffId);
        }

        public async Task<bool> AddTimeOffAsync(TimeOff timeOff)
        {
            try
            {
                await _context.TimeOffs.AddAsync(timeOff);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateTimeOffAsync(TimeOff timeOff)
        {
            try
            {
                _context.TimeOffs.Update(timeOff);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTimeOffAsync(int timeOffId)
        {
            try
            {
                var timeOff = await _context.TimeOffs.FindAsync(timeOffId);
                if (timeOff == null)
                    return false;

                _context.TimeOffs.Remove(timeOff);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> IsTimeOffExistsAsync(int doctorId, DateOnly startDate, DateOnly endDate, int? excludeTimeOffId = null)
        {
            var query = _context.TimeOffs
                .Where(t => t.DoctorUserId == doctorId &&
                           (t.StartDate <= startDate && t.EndDate >= startDate ||
                            t.StartDate <= endDate && t.EndDate >= endDate ||
                            t.StartDate >= startDate && t.EndDate <= endDate));

            if (excludeTimeOffId.HasValue)
            {
                query = query.Where(t => t.TimeOffId != excludeTimeOffId.Value);
            }

            return await query.AnyAsync();
        }
    }
}