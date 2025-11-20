using System;
using System.Collections.Generic;
namespace HealthBookingSystemAPI.DTOs
{
    public class AiconversationDTO
    {
        public int UserId { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool? IsActive { get; set; }

        public ICollection<AimessageDTO> Aimessages { get; set; } = new List<AimessageDTO>();

        public UserDTO User { get; set; } = null!;
    }
}