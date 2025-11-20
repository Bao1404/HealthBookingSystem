using BusinessObject.Models;
using System.Collections.Generic;

namespace Services.Interface
{
    public interface IDoctorService
    {
        IEnumerable<Doctor> GetAllDoctors();
        Doctor GetDoctorById(int id);
        void AddDoctor(Doctor doctor);
        void UpdateDoctor(Doctor doctor);
        void DeleteDoctor(int id);
        IEnumerable<Doctor> GetBySpecialty(int specialtyId);
        Task<Doctor> GetDoctorsByIdAsync(int id);
        Task<List<Doctor>> GetDoctorsAsync();
        IEnumerable<Doctor> GetBySpecialtyAsync(int specialtyId);
        Task UpdateImageUrlDoctor(string url, int userId);
    }
}
