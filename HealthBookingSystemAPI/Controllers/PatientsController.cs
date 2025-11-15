using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }
        [HttpGet("doctor/{id}")]
        public async Task<IActionResult> GetPatientsByDoctorId(int id)
        {
            var patients = await _patientService.GetPatientsByDoctorAsync(id);
            return Ok(patients);
        }
        [HttpGet("doctor/new/{id}")]
        public async Task<IActionResult> GetNewPatientsByDoctorId(int id)
        {
            var patients = await _patientService.GetNewPatientsAsync(id);
            return Ok(patients);
        }
    }
}
