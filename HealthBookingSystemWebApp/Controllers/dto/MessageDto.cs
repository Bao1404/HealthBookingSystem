public class MessageDTO
{
    public int MessageId { get; set; }
    public string Content { get; set; }
        public string MessageType { get; set; }
    public DateTime? SentAt { get; set; }
    public SenderDTO Sender { get; set; }
}

public class SenderDTO
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public string? AvatarUrl { get; set; }
}
public class SendMessageDto
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text"; 
}