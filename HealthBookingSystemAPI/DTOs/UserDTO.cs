using System;
using System.Collections.Generic;

namespace HealthBookingSystemAPI.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? Role { get; set; }

        public string FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool? IsActive { get; set; }

        public AiconversationDTO? Aiconversation { get; set; }

        //public ICollection<ConversationDTO> ConversationDoctorUsers { get; set; } = new List<ConversationDTO>();

        //public ICollection<ConversationDTO> ConversationPatientUsers { get; set; } = new List<ConversationDTO>();

        public DoctorDTO? Doctor { get; set; }

        public ICollection<MedicalRecordDTO> MedicalRecordDoctorUsers { get; set; } = new List<MedicalRecordDTO>();

        public ICollection<MedicalRecordDTO> MedicalRecordPatientUsers { get; set; } = new List<MedicalRecordDTO>();

        //public ICollection<Message> Messages { get; set; } = new List<Message>();

        public virtual PatientDTO? Patient { get; set; }

        //public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<PrescriptionDTO> PrescriptionDoctorUsers { get; set; } = new List<PrescriptionDTO>();

        public ICollection<PrescriptionDTO> PrescriptionPatientUsers { get; set; } = new List<PrescriptionDTO>();

        public ICollection<ReviewDTO> ReviewDoctorUsers { get; set; } = new List<ReviewDTO>();

        public ICollection<ReviewDTO> ReviewPatientUsers { get; set; } = new List<ReviewDTO>();
    }
}