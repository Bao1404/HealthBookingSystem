using System;
using System.Collections.Generic;


public class PatientDTO
{
    public int UserId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? BloodType { get; set; }

    public string? Allergies { get; set; }

    public int? Weight { get; set; }

    public int? Height { get; set; }

    public decimal? Bmi { get; set; }

    public string? Address { get; set; }

    public string? EmergencyPhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<AppointmentDTO> Appointments { get; set; } = new List<AppointmentDTO>();

    public ICollection<MedicalHistoryDTO> MedicalHistories { get; set; } = new List<MedicalHistoryDTO>();

    public UserDTO User { get; set; } = null!;
}

public class PatientUpdateDTO
{
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? EmergencyPhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public string? BloodType { get; set; }
    public int? Weight { get; set; }
    public int? Height { get; set; }
    public decimal? Bmi { get; set; }
}
