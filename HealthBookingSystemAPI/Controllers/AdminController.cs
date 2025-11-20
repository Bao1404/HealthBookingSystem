using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;
        public AdminController(IUserService userService, IAppointmentService appointmentService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
        }

        [HttpGet("ManageDoctors")]
        public async Task<IActionResult> ManageDoctors()
        {
            var users = await _userService.GetAllUsers();
            var doctors = users.Where(u => u.Role != null && u.Role.ToLower() == "doctor").ToList();
            return Ok(doctors);
        }

        [HttpGet("ManagePatients")]
        public async Task<IActionResult> ManagePatients()
        {
            var users = await _userService.GetAllUsers();
            var patients = users.Where(u => u.Role != null && u.Role.ToLower() == "patient").ToList();
            return Ok(patients);
        }
    }
}
