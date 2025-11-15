namespace HealthBookingSystemAPI.DTOs
{
    public class LoginRequestDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class AccountResponseDTO
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public int AccountId { get; set; }
    }
}
