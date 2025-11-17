namespace HealthBookingSystem.DTOs
{
    public class AiChatRequest
    {
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
