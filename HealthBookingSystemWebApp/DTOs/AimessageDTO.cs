using System;
using System.Collections.Generic;

public class AimessageDTO
{
    public int AimessageId { get; set; }

    public int UserId { get; set; }

    public string Sender { get; set; } = null!;

    public string? MessageType { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public bool? IsRead { get; set; }

    public AiconversationDTO User { get; set; } = null!;
}
