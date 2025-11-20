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
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }
        [HttpGet]
        [EnableQuery]
        public IActionResult GetAllDoctors()
        {
            var doctors = _doctorService.GetAllDoctors();
            return Ok(doctors);
        }
        [HttpGet("{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _doctorService.GetDoctorsByIdAsync(id);
            return Ok(doctor);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorUpdateDTO doctorDTO)
        {
            var doctor = await _doctorService.GetDoctorsByIdAsync(id);
            if (id != doctor.UserId)
            {
                return BadRequest("Doctor ID mismatch.");
            }
            if(doctorDTO.FullName != null)
            {
                doctor.User.FullName = doctorDTO.FullName;
            }
            if(doctorDTO.AvatarUrl != null)
            {
                doctor.User.AvatarUrl = doctorDTO.AvatarUrl;
            }
            if(doctorDTO.Bio != null)
            {
                doctor.Bio = doctorDTO.Bio;
            }
            if(doctorDTO.SpecialtyId != null)
            {
                doctor.SpecialtyId = doctorDTO.SpecialtyId;
            }
            if(doctorDTO.PhoneNumber != null)
            {
                doctor.User.PhoneNumber = doctorDTO.PhoneNumber;
            }
            if(doctorDTO.Experience != null)
            {
                doctor.Experience = doctorDTO.Experience;
            }
            if(doctorDTO.Password != null)
            {
                doctor.User.Password = doctorDTO.Password;
            }
            _doctorService.UpdateDoctor(doctor);
            return NoContent();
        }
    }
}
