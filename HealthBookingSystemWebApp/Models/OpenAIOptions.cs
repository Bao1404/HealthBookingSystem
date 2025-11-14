namespace HealthBookingSystem.Models
{
    public class OpenAIOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-2.0-flash";
    }
}
