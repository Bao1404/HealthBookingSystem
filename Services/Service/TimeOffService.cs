using BusinessObject.Models;
using Repositories.Interface;
using Services.Interface;

namespace Services.Service
{
    public class TimeOffService : ITimeOffService
    {
        private readonly ITimeOffRepository _timeOffRepository;

        public TimeOffService(ITimeOffRepository timeOffRepository)
        {
            _timeOffRepository = timeOffRepository;
        }

        public async Task<List<TimeOff>> GetTimeOffsByDoctorAsync(int doctorId)
        {
            return await _timeOffRepository.GetTimeOffsByDoctorAsync(doctorId);
        }

        public async Task<TimeOff?> GetTimeOffByIdAsync(int timeOffId)
        {
            return await _timeOffRepository.GetTimeOffByIdAsync(timeOffId);
        }

        public async Task<bool> AddTimeOffAsync(TimeOff timeOff)
        {
            try
            {
                timeOff.CreatedAt = DateTime.Now;
                timeOff.UpdatedAt = DateTime.Now;
                return await _timeOffRepository.AddTimeOffAsync(timeOff);
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
                timeOff.UpdatedAt = DateTime.Now;
                return await _timeOffRepository.UpdateTimeOffAsync(timeOff);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTimeOffAsync(int timeOffId)
        {
            return await _timeOffRepository.DeleteTimeOffAsync(timeOffId);
        }

        public async Task<bool> IsTimeOffExistsAsync(int doctorId, DateOnly startDate, DateOnly endDate, int? excludeTimeOffId = null)
        {
            return await _timeOffRepository.IsTimeOffExistsAsync(doctorId, startDate, endDate, excludeTimeOffId);
        }
    }
}