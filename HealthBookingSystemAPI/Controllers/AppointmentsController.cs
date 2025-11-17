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
        public async Task<IActionResult> GetAppointmentsByUserId()
        {
            var appointments = _appointmentService.GetAllAppointmentsAsync();
            return Ok(appointments);
        }
        [HttpGet("doctor/{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetAppointmentsByDoctorId(int id)
        {
            var appointments = _appointmentService.GetAppointmentsByDoctorId(id);
            return Ok(appointments);
        }
        [HttpGet("doctor-week/{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetAppointmentsByDoctorAndWeek(int id)
        {
            var appointments = _appointmentService.GetAppointmentsByWeekAsync(id);
            return Ok(appointments);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO appointmentDTO)
        {
            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
            if(appointment == null)
            {
                return NotFound("Appointment not found.");
            }
            appointment.Status = appointmentDTO.Status;
            await _appointmentService.UpdateAppointmentAsync(appointment);
            return NoContent();
        }
    }
}
