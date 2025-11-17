using System;
using System.Collections.Generic;

public partial class PrescriptionDTO
{
    public int PrescriptionId { get; set; }

    public int RecordId { get; set; }

    public int PatientUserId { get; set; }

    public int DoctorUserId { get; set; }

    public string? Medication { get; set; }

    public string? Instructions { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public UserDTO DoctorUser { get; set; } = null!;

    public UserDTO PatientUser { get; set; } = null!;

    public MedicalRecordDTO Record { get; set; } = null!;
}
