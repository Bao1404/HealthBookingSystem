using BusinessObject.Models;
using HealthCareSystem.Controllers.dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Repositories.IRepositories;

namespace HealthCareSystem.Controllers
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

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetConversationsByDoctor(int doctorId)
        {
            try
            {
                var conversations = await _conversationRepository.GetConversationsByDoctorId(doctorId);

                var result = conversations.Select(c => new ConversationDto
                {
                    ConversationId = c.ConversationId,
                    PatientUserId = c.PatientUserId,
                    DoctorUserId = c.DoctorUserId,
                    UpdatedAt = c.UpdatedAt,
                    PatientUser = new UserDto
                    {
                        UserId = c.PatientUser.UserId,
                        Email = c.PatientUser.Email,
                        FullName = c.PatientUser.FullName,
                        Role = c.PatientUser.Role,
                        PhoneNumber = c.PatientUser.PhoneNumber,
                        AvatarUrl = c.PatientUser.AvatarUrl
                    },
                    DoctorUser = new UserDto
                    {
                        UserId = c.DoctorUser.UserId,
                        Email = c.DoctorUser.Email,
                        FullName = c.DoctorUser.FullName,
                        Role = c.DoctorUser.Role,
                        PhoneNumber = c.DoctorUser.PhoneNumber,
                        AvatarUrl = c.DoctorUser.AvatarUrl
                    }
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpGet("patient/{patientID}")]
        public async Task<IActionResult> GetConversationsByPatient(int patientID)
        {
            try
            {
                var conversations = await _conversationRepository.GetConversationsByPatientId(patientID);

                var result = conversations.Select(c => new ConversationDto
                {
                    ConversationId = c.ConversationId,
                    PatientUserId = c.PatientUserId,
                    DoctorUserId = c.DoctorUserId,
                    UpdatedAt = c.UpdatedAt,
                    PatientUser = new UserDto
                    {
                        UserId = c.PatientUser.UserId,
                        Email = c.PatientUser.Email,
                        FullName = c.PatientUser.FullName,
                        Role = c.PatientUser.Role,
                        PhoneNumber = c.PatientUser.PhoneNumber,
                        AvatarUrl = c.PatientUser.AvatarUrl
                    },
                    DoctorUser = new UserDto
                    {
                        UserId = c.DoctorUser.UserId,
                        Email = c.DoctorUser.Email,
                        FullName = c.DoctorUser.FullName,
                        Role = c.DoctorUser.Role,
                        PhoneNumber = c.DoctorUser.PhoneNumber,
                        AvatarUrl = c.DoctorUser.AvatarUrl
                    }
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto model)
        {
            try
            {
                // Tạo một cuộc trò chuyện mới
                //var conversation = new ConversationDTO
                //{
                //    PatientUserId = model.PatientUserId,
                //    DoctorUserId = model.DoctorUserId,
                //    CreatedAt = DateTime.UtcNow,
                //    UpdatedAt = DateTime.UtcNow
                //};

                //await _conversationRepository.CreateAsync(conversation);

                //return Ok(conversation);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi từ máy chủ: {ex.Message}");
            }
        }
        [HttpGet("find-or-create")]
        public async Task<IActionResult> FindOrCreateConversation(int doctorId)
        {
            var patientId = HttpContext.Session.GetInt32("UserId");

            if (patientId == null)
            {
                return Unauthorized(); // Trả về lỗi nếu bệnh nhân chưa đăng nhập
            }

            try
            {
                // Tìm cuộc trò chuyện giữa bệnh nhân và bác sĩ
                var conversation = await _conversationRepository.FindConversationByPatientIdAndDoctorId((int)patientId, doctorId);

                if (conversation != null)
                {
                    // Nếu đã có cuộc trò chuyện, điều hướng đến cuộc trò chuyện đó
                    return Ok(new { success = true, conversationId = conversation.ConversationId });
                }
                else
                {
                    // Nếu không có cuộc trò chuyện, tạo cuộc trò chuyện mới
                    //var newConversation = new ConversationDTO
                    //{
                    //    PatientUserId = (int)patientId,
                    //    DoctorUserId = doctorId,
                    //    CreatedAt = DateTime.UtcNow,
                    //    UpdatedAt = DateTime.UtcNow
                    //};

                    //// Tạo cuộc trò chuyện mới
                    //var createdConversation = await _conversationRepository.CreateAsync(newConversation);

                    // Trả về thông tin cuộc trò chuyện mới đã tạo
                    //return Ok(new { success = false, conversationId = createdConversation.ConversationId });
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết nếu có
               
                return StatusCode(500, new { error = "Lỗi server: " + ex.Message });
            }
        }




    }
}
