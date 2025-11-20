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
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = _patientService.GetAllPatients();
            return Ok(patients);
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
        [HttpGet("doctor/{id}/search")]
        [EnableQuery]
        public IActionResult SearchPatients(int id, [FromQuery] string searchTerm)
        {
            var patients = _patientService.SearchPatientsAsync(id, searchTerm);
            return Ok(patients);
        }
        [HttpGet("{id}")]
        [EnableQuery(MaxExpansionDepth = 5)]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = _patientService.GetPatientById(id);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
    }
}
