namespace HealthBookingSystemAPI.DTOs
{
    public class UpdateAppointmentDTO
    {
        public int AppointmentId { get; set; }
        public string Status { get; set; }
    }
    public class AddAppointmentDTO
    {
        public int PatientUserId { get; set; }
        public int DoctorUserId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string? Notes { get; set; }
    }
    public class AppointmentDTO
    {
        public int AppointmentId { get; set; }

        public int PatientUserId { get; set; }

        public int DoctorUserId { get; set; }

        public DateTime AppointmentDateTime { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? Notes { get; set; }

        public DoctorDTO DoctorUser { get; set; } = null!;

        public ICollection<MedicalRecordDTO> MedicalRecords { get; set; } = new List<MedicalRecordDTO>();

        public PatientDTO PatientUser { get; set; } = null!;
    }
}
