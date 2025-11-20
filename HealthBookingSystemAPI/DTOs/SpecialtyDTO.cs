using System;
using System.Collections.Generic;

namespace HealthBookingSystemAPI.DTOs
{
    public class SpecialtyDTO
    {
        public int SpecialtyId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ICollection<DoctorDTO> Doctors { get; set; } = new List<DoctorDTO>();
    }
}