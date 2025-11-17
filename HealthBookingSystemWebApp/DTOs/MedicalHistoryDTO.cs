using System;
using System.Collections.Generic;

public class MedicalHistoryDTO
{
    public int HistoryId { get; set; }

    public int PatientUserId { get; set; }

    public string ConditionName { get; set; } = null!;

    public PatientDTO PatientUser { get; set; } = null!;
}
