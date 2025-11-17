using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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
        [EnableQuery]
        public async Task<IActionResult> GetPatientsByDoctorId(int id)
        {
            var patients = _patientService.GetPatientsByDoctorAsync(id);
            return Ok(patients);
        }
        [HttpGet("doctor/new/{id}")]
        [EnableQuery]
        public IActionResult GetNewPatientsByDoctorId(int id)
        {
            var patients = _patientService.GetNewPatientsAsync(id);
            return Ok(patients);
        }
    }
}
