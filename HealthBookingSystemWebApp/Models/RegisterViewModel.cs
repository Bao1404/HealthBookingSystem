using System.ComponentModel.DataAnnotations;

namespace HealthBookingSystem.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Username must be at least 3 characters long.", MinimumLength = 3)]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public DateOnly Dob { get; set; }
        [Required]
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string? BloodType { get; set; }
        [Required]
        public string EmergencyContact { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public int Height { get; set; }
        public decimal Bmi { get; set; }
        public string? Allergies { get; set; }
    }
}
