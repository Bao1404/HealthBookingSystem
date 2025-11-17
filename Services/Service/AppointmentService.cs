using BusinessObject.Models;
using Repositories.Interface;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }
        public async Task<List<Appointment>> GetAllAppointmentsAsync() => await _appointmentRepository.GetAllAppointmentsAsync();
        public async Task<Appointment?> GetAppointmentsByIdAsync(int id) => await _appointmentRepository.GetAppointmentsByIdAsync(id);
        public async Task UpdateAppointmentAsync(Appointment appointment) => await _appointmentRepository.UpdateAppointmentAsync(appointment);
        public async Task DeleteAppointmentAsync(int id) => await _appointmentRepository.DeleteAppointmentAsync(id);
        public async Task<bool> IsTimeSlotBookedAsync(int doctorId, DateTime dateTime) => 
            await _appointmentRepository.IsTimeSlotBookedAsync(doctorId, dateTime);
        public async Task<bool> IsTimeSlotBookedAsync(int doctorId, DateTime dateTime, int excludeAppointmentId) =>
            await _appointmentRepository.IsTimeSlotBookedAsync(doctorId, dateTime, excludeAppointmentId);
        public async Task<List<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateTime date) =>
            await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, date);

        // Doctor-specific methods implementation
        public async Task<List<Appointment>> GetPendingAppointmentsByDoctorAsync(int doctorId) =>
            await _appointmentRepository.GetPendingAppointmentsByDoctorAsync(doctorId);

        public async Task<List<Appointment>> GetTodayAppointmentsByDoctorAsync(int doctorId) =>
            await _appointmentRepository.GetTodayAppointmentsByDoctorAsync(doctorId);

        public async Task<List<Appointment>> GetUpcomingAppointmentsByDoctorAsync(int doctorId) =>
            await _appointmentRepository.GetUpcomingAppointmentsByDoctorAsync(doctorId);

        public async Task<List<Appointment>> GetCompletedAppointmentsByDoctorAsync(int doctorId) =>
            await _appointmentRepository.GetCompletedAppointmentsByDoctorAsync(doctorId);

        public async Task<List<Appointment>> GetCancelledAppointmentsByDoctorAsync(int doctorId) =>
            await _appointmentRepository.GetCancelledAppointmentsByDoctorAsync(doctorId);

        public async Task<List<Appointment>> GetAppointmentsByDoctorAndStatusAsync(int doctorId, string status) =>
            await _appointmentRepository.GetAppointmentsByDoctorAndStatusAsync(doctorId, status);

        public IEnumerable<Appointment> GetAppointmentsByWeekAsync(int doctorId) =>
            _appointmentRepository.GetAppointmentsByWeekAsync(doctorId);

        public async Task<List<Appointment>> GetAppointmentsByMonthAsync(int doctorId, DateTime monthStart) =>
            await _appointmentRepository.GetAppointmentsByMonthAsync(doctorId, monthStart);
        public async Task<List<Appointment>> GetAppointmentsByDateAsync(int doctorId, DateTime date) =>
            await _appointmentRepository.GetAppointmentsByDateAsync(doctorId, date);
        public IEnumerable<Appointment> GetAppointmentsByDoctorId(int doctorId) => _appointmentRepository.GetAppointmentsByDoctorId(doctorId);
        public async Task<bool> ApproveAppointmentAsync(int appointmentId, int doctorId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentsByIdAsync(appointmentId);
                if (appointment == null || appointment.DoctorUserId != doctorId)
                    return false;

                appointment.Status = "Confirmed";
                appointment.UpdatedAt = DateTime.Now;
                await _appointmentRepository.UpdateAppointmentAsync(appointment);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectAppointmentAsync(int appointmentId, int doctorId, string? reason = null)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentsByIdAsync(appointmentId);
                if (appointment == null || appointment.DoctorUserId != doctorId)
                    return false;

                appointment.Status = "Cancelled";
                appointment.UpdatedAt = DateTime.Now;
                if (!string.IsNullOrEmpty(reason))
                {
                    appointment.Notes = $"Cancelled by doctor: {reason}";
                }
                await _appointmentRepository.UpdateAppointmentAsync(appointment);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _appointmentRepository.AddAppointmentAsync(appointment);
        }

    }
}
