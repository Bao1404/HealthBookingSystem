using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentsByIdAsync(int id);
        Task AddAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task DeleteAppointmentAsync(int id);
        Task<bool> IsTimeSlotBookedAsync(int doctorId, DateTime dateTime);
        Task<bool> IsTimeSlotBookedAsync(int doctorId, DateTime dateTime, int excludeAppointmentId);
        Task<List<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateTime date);

        // Doctor-specific methods
        Task<List<Appointment>> GetPendingAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetTodayAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetUpcomingAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetCompletedAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetCancelledAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetAppointmentsByDoctorAndStatusAsync(int doctorId, string status);
        IEnumerable<Appointment> GetAppointmentsByDoctorId(int doctorId);
        Task<bool> ApproveAppointmentAsync(int appointmentId, int doctorId);
        Task<bool> RejectAppointmentAsync(int appointmentId, int doctorId, string? reason = null);
        IEnumerable<Appointment> GetAppointmentsByWeekAsync(int doctorId);
        IEnumerable<Appointment> GetAppointmentsByMonthAsync(int doctorId);
        Task<List<Appointment>> GetAppointmentsByDateAsync(int doctorId, DateTime date);
    }
}
