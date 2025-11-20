namespace HealthBookingSystemWebApp.DTOs
{
    public class RegisterDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public DateOnly Dob { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string? BloodType { get; set; }
        public string EmergencyContact { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }
        public decimal Bmi { get; set; }
    }
}
