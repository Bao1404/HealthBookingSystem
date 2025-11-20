using System.ComponentModel.DataAnnotations;

namespace HealthBookingSystemAPI.DTOs
{
    public class RegisterPatientDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public DateOnly Dob { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string? BloodType { get; set; }
        public string EmergencyContact { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }
        public decimal Bmi { get; set; }
        public string? Allergies { get; set; }
    }
    public class RegisterDoctorDTO
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public string? Experience { get; set; }
        public string? Bio { get; set; }
        public int SpecialtyId { get; set; }
    }
}
