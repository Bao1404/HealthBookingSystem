using System;
using System.Collections.Generic;


public class MedicalRecordDTO
{
    public int RecordId { get; set; }

    public int PatientUserId { get; set; }

    public int DoctorUserId { get; set; }

    public int? AppointmentId { get; set; }

    public string? Diagnosis { get; set; }

    public string? Treatment { get; set; }

    public string? TestResults { get; set; }

    public string? MedicalImages { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public AppointmentDTO? Appointment { get; set; }

    public UserDTO DoctorUser { get; set; } = null!;

    public UserDTO PatientUser { get; set; } = null!;

    public ICollection<PrescriptionDTO> Prescriptions { get; set; } = new List<PrescriptionDTO>();
}
