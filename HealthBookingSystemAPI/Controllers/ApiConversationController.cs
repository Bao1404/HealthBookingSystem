using BusinessObject.Models;
using HealthBookingSystemAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiConversationController : ControllerBase
    {
        private readonly IConversationRepository _conversationRepository;
        public ApiConversationController(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }
        [HttpGet("GetConversationsByDoctor/{doctorId}")]
        public async Task<IActionResult> GetConversationsByDoctor(int doctorId)
        {
            return Ok(await _conversationRepository.GetConversationsByDoctorId(doctorId));
        }

        [HttpGet("GetConversationsByPatient/{patientId}")]
        public async Task<IActionResult> GetConversationsByPatient(int patientId)
        {
            return Ok(await _conversationRepository.GetConversationsByPatientId(patientId));
        }

        [HttpPost("FindConversationByPatientIdAndDoctorId")]
        public async Task<IActionResult> FindConversationByPatientIdAndDoctorId(int patientId, int doctorId)
        {
           return Ok( await _conversationRepository.FindConversationByPatientIdAndDoctorId((int)patientId, doctorId));
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> CreateConversation(Conversation conversation)
        //{
        //    return Ok(await _conversationRepository.CreateAsync(conversation));
        //}
        //[HttpPut("{id}")]
        //[HttpDelete("{id}")]
    }
}
