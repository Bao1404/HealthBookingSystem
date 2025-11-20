using System;
using System.Collections.Generic;

namespace HealthBookingSystemAPI.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }

        public int PatientUserId { get; set; }

        public int DoctorUserId { get; set; }

        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public UserDTO DoctorUser { get; set; } = null!;

        public UserDTO PatientUser { get; set; } = null!;
    }
}