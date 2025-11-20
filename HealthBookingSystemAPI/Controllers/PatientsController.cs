using HealthBookingSystemAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services.Interface;
using Services.Service;

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpdateDTO patientDTO)
        {
            var patient = _patientService.GetPatientById(id);
            if (id != patient.UserId)
            {
                return BadRequest("Doctor ID mismatch.");
            }
            if (patientDTO.FullName != null)
            {
                patient.User.FullName = patientDTO.FullName;
            }
            if (patientDTO.Email != null)
            {
                patient.User.Email = patientDTO.Email;
            }
            if (patientDTO.EmergencyPhoneNumber != null)
            {
                patient.EmergencyPhoneNumber = patientDTO.EmergencyPhoneNumber;
            }
            if (patientDTO.Phone != null)
            {
                patient.User.PhoneNumber = patientDTO.Phone;
            }
            if (patientDTO.DateOfBirth != null)
            {
                patient.DateOfBirth = patientDTO.DateOfBirth;
            }
            if (patientDTO.Address != null)
            {
                patient.Address = patientDTO.Address;
            }
            if (patientDTO.Gender != null)
            {
                patient.Gender = patient.Gender;
            }
            if (patientDTO.BloodType != null)
            {
                patient.BloodType = patientDTO.BloodType;
            }
            if (patientDTO.Weight != null)
            {
                patient.Weight = patientDTO.Weight;
            }
            if (patientDTO.Height != null)
            {
                patient.Height = patientDTO.Height;
            }
            if (patientDTO.Bmi != null)
            {
                patient.Bmi = patientDTO.Bmi;
            }
            if (patientDTO.AvatarUrl != null)
            {
                patient.User.AvatarUrl = patientDTO.AvatarUrl;
            }

            await _patientService.UpdatePatient(patient);
            return NoContent();
        }
    }
}
