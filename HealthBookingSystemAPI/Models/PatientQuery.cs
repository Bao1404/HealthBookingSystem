namespace HealthBookingSystemAPI.Models
{
    public class PatientQuery
    {
        public int? DoctorId { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public bool? IsCritical { get; set; }
        public bool? IsFollowUp { get; set; }
        public int? NewWithinDays { get; set; }
        public bool? IsActive { get; set; }
    }
}
