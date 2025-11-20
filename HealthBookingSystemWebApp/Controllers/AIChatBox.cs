using BusinessObject.Models;
using HealthBookingSystem.DTOs;
using HealthBookingSystem.Mapper;
using HealthBookingSystem.Models;
using HealthBookingSystemWebApp.DTOs;
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
        private readonly ISpecialtyService _specialtyService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAIOptions _option;
        private readonly HttpClient _httpClient;
        public AIChatBox(IAiConversationService aiConversationService, IAiMessageService aiMessageService, IOptions<OpenAIOptions> option, IHttpClientFactory httpClientFactory, ISpecialtyService specialtyService)
        {
            _aiConversationService = aiConversationService;
            _aiMessageService = aiMessageService;
            _option = option.Value;
            _httpClientFactory = httpClientFactory;
            _specialtyService = specialtyService;
            _httpClient = httpClientFactory.CreateClient("APIClient");
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
            var reqMsg = new HttpRequestMessage(HttpMethod.Post, url);
            reqMsg.Content = content;

            var resp = await SendWithRetry(client, reqMsg);

            if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return StatusCode(429, new
                {
                    aiReply = "Hệ thống AI đang quá tải. Vui lòng thử lại sau vài giây.",
                    recommendedDoctors = new List<object>()
                });
            }

            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new
                {
                    aiReply = "AI hiện không thể trả lời. Vui lòng thử lại.",
                    recommendedDoctors = new List<object>()
                });
            }

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
            var docs = await GetBySpecialty(getSpecialty.SpecialtyId);
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
        private async Task<List<DoctorDTO>> GetBySpecialty(int id)
        {
            var request = await _httpClient.GetAsync($"Doctors/speciality/{id}?$expand=User,Specialty");
            if (request.IsSuccessStatusCode)
            {
                var doctors = await request.Content.ReadFromJsonAsync<List<DoctorDTO>>();
                return doctors ?? new List<DoctorDTO>();
            }
            return new List<DoctorDTO>();
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
        private async Task<HttpResponseMessage> SendWithRetry(HttpClient client, HttpRequestMessage request)
        {
            for (int i = 0; i < 3; i++)
            {
                var response = await client.SendAsync(request);

                if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                    return response; // thành công -> trả về luôn

                // Nếu API trả về 429, kiểm tra xem có header Retry-After không
                if (response.Headers.TryGetValues("Retry-After", out var values))
                {
                    int seconds = int.Parse(values.First());
                    await Task.Delay(seconds * 1000);
                }
                else
                {
                    await Task.Delay(2000); // Retry sau 2 giây
                }
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.TooManyRequests);
        }

    }
}
