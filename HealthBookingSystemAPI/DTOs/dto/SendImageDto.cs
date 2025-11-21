public class SendImageDto
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public IFormFile File { get; set; } // Tệp ảnh
}
