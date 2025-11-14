using BusinessObject.Models;
using System.Collections.Generic;

namespace Repositories.Interface
{
    public interface IPatientRepository
    {
        IEnumerable<Patient> GetAll();
        Patient GetById(int id);
        void Add(Patient patient);
        Task Update(Patient patient);
        void Delete(int id);
        Task CreatePatient(Patient patient);

        Task<Patient?> GetByUserIdAsync(int userId);

        Task<List<Patient>> GetAllPatientsAsync();
        Task<List<Patient>> GetPatientsByDoctorAsync(int doctorId);
        Task<Patient?> GetPatientWithDetailsAsync(int userId);
        Task<List<Patient>> GetPatientsByDoctorAndStatusAsync(int doctorId, string status);
        Task<List<Patient>> SearchPatientsAsync(int doctorId, string searchTerm);
        Task<List<Patient>> GetCriticalPatientsAsync(int doctorId);
        Task<List<Patient>> GetFollowUpPatientsAsync(int doctorId);
        Task<List<Patient>> GetNewPatientsAsync(int doctorId, int daysThreshold = 30);
        Task<List<Patient>> GetActivePatientsAsync(int doctorId);
        Task UpdateImageUrlPatient(string url, int userId);
    }
}
