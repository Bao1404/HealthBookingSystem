using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Service;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalHistoriesController : ControllerBase
    {
        private readonly MedicalHistoriesService _medicalHistoriesService;
        public MedicalHistoriesController(MedicalHistoriesService medicalHistoriesService)
        {
            _medicalHistoriesService = medicalHistoriesService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicalHistoryById(int id)
        {
            var medicalHistory = await _medicalHistoriesService.GetHistoryByUserId(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }
            return Ok(medicalHistory);
        }
    }
}
