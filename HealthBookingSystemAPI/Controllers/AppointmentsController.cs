using BusinessObject.Models;
using HealthBookingSystemAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services.Interface;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = _appointmentService.GetAllAppointments();
            return Ok(appointments);
        }
        [HttpGet("user/{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetAppointmentsByUserId(int id)
        {
            var appointments = _appointmentService.GetAppointmentsByUserId(id);
            return Ok(appointments);
        }
        [HttpGet("week/{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetAppointmentsByWeek(int id)
        {
            var appointments = _appointmentService.GetAppointmentsByWeekAsync(id);
            return Ok(appointments);
        }
        [HttpGet("month/{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetAppointmentsByMonth(int id)
        {
            var appointments = _appointmentService.GetAppointmentsByMonthAsync(id);
            return Ok(appointments);
        }
        [HttpGet("{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }
            return Ok(appointment);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO appointmentDTO)
        {
            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }
            appointment.Status = appointmentDTO.Status;
            await _appointmentService.UpdateAppointmentAsync(appointment);
            return NoContent();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AddAppointmentDTO appointmentDTO)
        {
            var appointment = new Appointment
            {
                PatientUserId = appointmentDTO.PatientUserId,
                DoctorUserId = appointmentDTO.DoctorUserId,
                Notes = appointmentDTO.Notes,
                Status = "Upcoming",
                AppointmentDateTime = appointmentDTO.AppointmentDateTime,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _appointmentService.AddAppointmentAsync(appointment);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.AppointmentId }, appointment);
        }
        [HttpPost("GetUpcomingAppointments")]
        public async Task<IActionResult> GetUpcomingAppointments([FromBody] List<AppointmentDTO> appointments)
        {
            return Ok(appointments
                .Where(a => a.AppointmentDateTime > DateTime.Now && (a.Status == "Pending" || a.Status == "Confirmed"))
                .OrderBy(a => a.AppointmentDateTime)
                .Take(5)
                .ToList());
        }

        [HttpPost("GetRecentAppointments")]
        public async Task<IActionResult> GetRecentAppointments(List<AppointmentDTO> appointments)
        {
            return Ok(appointments
                .Where(a => a.AppointmentDateTime <= DateTime.Now)
                .OrderByDescending(a => a.AppointmentDateTime)
                .Take(10)
                .ToList());
        }

        [HttpPost("GetTodayAppointments")]
        public async Task<IActionResult> GetTodayAppointments(List<AppointmentDTO> appointments)
        {
            return Ok(appointments
                .Where(a => a.AppointmentDateTime.Date == DateTime.Today && (a.Status == "Pending" || a.Status == "Confirmed"))
                .OrderBy(a => a.AppointmentDateTime)
                .ToList());
        }

        [HttpPost("GetAvailableTimeSlots")]
        public async Task<IActionResult> IsTimeSlotBookedAsync([FromQuery] int doctorId, [FromQuery] DateTime appointmentDateTime)
        {
            return Ok(await _appointmentService.IsTimeSlotBookedAsync(doctorId, appointmentDateTime));
        }

        [HttpPost("GetAvailableTimeSlotsForReschedule")]
        public async Task<IActionResult> IsTimeSlotBookedAsync([FromQuery] int doctorId, [FromQuery] DateTime appointmentDateTime, [FromQuery] int excludeAppointmentId)
        {
            return Ok(await _appointmentService.IsTimeSlotBookedAsync(doctorId, appointmentDateTime, excludeAppointmentId));
        }
    }
}
