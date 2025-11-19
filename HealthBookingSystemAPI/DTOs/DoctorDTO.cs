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
}
