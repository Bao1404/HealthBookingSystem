using HealthCareSystem.Controllers.dto;

public class ConversationDto
{
    public int ConversationId { get; set; }
    public int PatientUserId { get; set; }
    public int DoctorUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserDto? PatientUser { get; set; }
    public UserDto? DoctorUser { get; set; }
}
