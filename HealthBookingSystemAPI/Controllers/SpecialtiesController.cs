using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services.Interface;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ISpecialtyService _specialtyService;
        public SpecialtiesController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetAllSpecialties()
        {
            var specialties = _specialtyService.GetAllSpecialtiesAsync();
            return Ok(specialties);
        }
    }
}
