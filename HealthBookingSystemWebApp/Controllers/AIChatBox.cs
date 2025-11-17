using BusinessObject.Models;
using HealthBookingSystem.DTOs;
using HealthBookingSystem.Mapper;
using HealthBookingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services;
using Services.Interface;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HealthBookingSystem.Controllers
{
    [Route("api/chatbox")]
    [ApiController]
    public class AIChatBox : ControllerBase
    {
        private readonly IAiConversationService _aiConversationService;
        private readonly IAiMessageService _aiMessageService;
        private readonly IDoctorService _doctorService;
        private readonly ISpecialtyService _specialtyService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAIOptions _option;
        public AIChatBox(IAiConversationService aiConversationService, IAiMessageService aiMessageService, IOptions<OpenAIOptions> option, IHttpClientFactory httpClientFactory, IDoctorService doctorService, ISpecialtyService specialtyService)
        {
            _aiConversationService = aiConversationService;
            _aiMessageService = aiMessageService;
            _option = option.Value;
            _httpClientFactory = httpClientFactory;
            _doctorService = doctorService;
            _specialtyService = specialtyService;
        }
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] AiChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message is required!");

            if (request.UserId == 0)
                return Unauthorized("User is not logged in.");

            // Get or create conversation
            var conversation = await _aiConversationService.GetConversationByUserId(request.UserId);
            if (conversation == null)
            {
                conversation = new Aiconversation
                {
                    UserId = request.UserId,
                    StartedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true
                };
                await _aiConversationService.CreateConversation(conversation);
            }

            // Fetch last 5-10 messages to create context for the AI (chat history)
            var messages = await _aiMessageService.GetMessagesByUserId(request.UserId);
            var lastMessages = messages.OrderBy(m => m.SentAt).TakeLast(5).ToList();  // You can adjust the number of messages here (e.g., 5)

            // Prepare history for Gemini API
            var parts = new List<object>
    {
        new { text = "You are a health AI assistant. Respond briefly to health, medical, wellness, or fitness-related queries." }
    };

            foreach (var msg in lastMessages)
            {
                parts.Add(new { text = msg.Content });
            }

            parts.Add(new { text = request.Message });

            var payload = new { contents = new[] { new { parts } } };
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var url = $"{_option.Endpoint}?key={_option.ApiKey}";
            var resp = await client.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();  // Ensure the response is successful

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());

            // Parse the response from Gemini API
            var aiReply = doc.RootElement
                                                 .GetProperty("candidates")[0]
                                                 .GetProperty("content")
                                                 .GetProperty("parts")[0]
                                                 .GetProperty("text")
                                                 .GetString()
                                             ?? "Xin lỗi, tôi không hiểu câu hỏi.";

            // Save messages to database
            var now = DateTime.Now;
            await _aiMessageService.SaveMessage(new[]
            {
        new Aimessage
        {
            UserId = request.UserId,
            Sender = "User",
            Content = request.Message,
            SentAt = now,
            IsRead = true
        },
        new Aimessage
        {
            UserId = request.UserId,
            Sender = "AI",
            Content = aiReply,
            SentAt = now,
            IsRead = false
        }
    });

            // Update the conversation timestamp
            conversation.UpdatedAt = now;
            await _aiConversationService.UpdateConversation(conversation);

            // Logic for getting the specialty and doctors
            var specialty = SpecialtyMapper.GetSpecialty(request.Message);
            var getSpecialty = await _specialtyService.GetSpecialtyByName(specialty);
            if(getSpecialty == null)
            {
                return Ok(new
                {
                    aiReply
                });
            }
            var docs = await _doctorService.GetBySpecialtyAsync(getSpecialty.SpecialtyId);
            var recommendedDoctors = docs.Select(d => new
            {
                UserId = d.UserId,
                SpecialtyId = d.Specialty?.SpecialtyId,
                FullName = d.User.FullName,
                Avatar = d.User.AvatarUrl,
                Specialty = d.Specialty?.Name,
                Experience = d.Experience
            }).ToList();

            Console.WriteLine(JsonSerializer.Serialize(recommendedDoctors));

            return Ok(new
            {
                aiReply,
                recommendedDoctors
            });
        }



        [HttpGet("messages/{userId}")]
        public async Task<IActionResult> GetMessages(int userId)
        {
            var messages = await _aiMessageService.GetMessagesByUserId(userId);
            if (messages == null)
            {
                return NotFound("No messages found for this user.");
            }

            return Ok(messages);
        }
        [HttpDelete("messages/delete/{userId}")]
        public async Task<IActionResult> DeleteHistory(int userId)
        {
            if(userId != 0)
            {
                await _aiMessageService.DeleteMessageByConversationId(userId);
                return Ok();
            }
            return BadRequest();
        }
    }
}
