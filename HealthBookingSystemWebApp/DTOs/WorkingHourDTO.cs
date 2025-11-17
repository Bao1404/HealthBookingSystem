using HealthBookingSystemWebApp.DTOs;
using System;
using System.Collections.Generic;

public class WorkingHourDTO
{
    public int WorkingHoursId { get; set; }

    public int DoctorUserId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public bool? IsWorking { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DoctorDTO DoctorUser { get; set; } = null!;
}
