namespace HealthBookingSystemAPI.DTOs
{
    public class DoctorUpdateDTO
    {
        public string? AvatarUrl { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public int? SpecialtyId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Experience { get; set; }
        public string? Password { get; set; }
    }

    public class DoctorDTO
    {
        public int UserId { get; set; }

        public int? SpecialtyId { get; set; }

        public string? Qualifications { get; set; }

        public string? Experience { get; set; }

        public string? Bio { get; set; }

        public decimal? Rating { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ICollection<AppointmentDTO> Appointments { get; set; } = new List<AppointmentDTO>();

        public SpecialtyDTO? Specialty { get; set; }

        public ICollection<TimeOffDTO> TimeOffs { get; set; } = new List<TimeOffDTO>();

        public UserDTO User { get; set; } = null!;

        public ICollection<WorkingHourDTO> WorkingHours { get; set; } = new List<WorkingHourDTO>();
    }
}
