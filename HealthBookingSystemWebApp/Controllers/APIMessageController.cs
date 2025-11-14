using BusinessObject.Models;
using HealthBookingSystem.Service;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;


namespace HealthCareSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIMessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly PhotoService _photoService;

        public APIMessageController(IMessageRepository messageRepository, PhotoService photoService)
        {
            _messageRepository = messageRepository;
            _photoService = photoService;
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<IActionResult> GetMessagesByConversationId(int conversationId)
        {
            var messages = await _messageRepository.GetMessagesByConversationId(conversationId);

            var result = messages.Select(m => new MessageDTO
            {
                MessageId = m.MessageId,
                Content = m.Content,
                MessageType = m.MessageType,
                SentAt = m.SentAt,
                Sender = new SenderDTO
                {
                    UserId = m.Sender.UserId,
                    FullName = m.Sender.FullName,
                    Role = m.Sender.Role,
                    AvatarUrl = m.Sender.AvatarUrl
                }
            }).ToList();

            return Ok(result);
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = dto.SenderId,
                Content = dto.Content,
                MessageType = dto.MessageType,
                SentAt = DateTime.Now,
                IsRead = false
            };

            await _messageRepository.CreateMessage(message);
            return Ok(message);
        }
        [HttpPost("send-image")]
        public async Task<IActionResult> SendImage([FromForm] SendImageDto dto)
        {
            if (dto.File.Length <= 0) return BadRequest("No file uploaded");

            // Tải ảnh lên Cloudinary
            var imageUrl = await _photoService.UploadImageAsync(dto.File);

            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = dto.SenderId,
                Content = imageUrl,  // Lưu URL ảnh vào nội dung
                MessageType = "image",  // Đặt kiểu tin nhắn là hình ảnh
                SentAt = DateTime.Now,
                IsRead = false
            };

            await _messageRepository.CreateMessage(message);
            return Ok(message);
        }


    }
}
