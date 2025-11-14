using BusinessObject.Models;
namespace Services.Interface
{
    public interface ITimeOffService
    {
        Task<List<TimeOff>> GetTimeOffsByDoctorAsync(int doctorId);
        Task<TimeOff?> GetTimeOffByIdAsync(int timeOffId);
        Task<bool> AddTimeOffAsync(TimeOff timeOff);
        Task<bool> UpdateTimeOffAsync(TimeOff timeOff);
        Task<bool> DeleteTimeOffAsync(int timeOffId);
        Task<bool> IsTimeOffExistsAsync(int doctorId, DateOnly startDate, DateOnly endDate, int? excludeTimeOffId = null);
    }
}