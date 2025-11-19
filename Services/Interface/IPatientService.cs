using BusinessObject.Models;
using System.Collections.Generic;

namespace Services.Interface
{
    public interface IPatientService
    {
        IEnumerable<Patient> GetAllPatients();
        Patient GetPatientById(int id);
        void AddPatient(Patient patient);
        Task UpdatePatient(Patient patient);
        void DeletePatient(int id);
        Patient GetByUserId(int userId);
        Task CreatePatient(Patient patient);
        Task<Patient?> GetByUserIdAsync(int userId);
        Task<List<Patient>> GetAllPatientsAsync();
        IEnumerable<Patient> GetPatientsByDoctorAsync(int doctorId);
        Task<Patient?> GetPatientWithDetailsAsync(int userId);
        Task<List<Patient>> GetPatientsByDoctorAndStatusAsync(int doctorId, string status);
        IEnumerable<Patient> SearchPatientsAsync(int doctorId, string searchTerm);
        Task<List<Patient>> GetCriticalPatientsAsync(int doctorId);
        Task<List<Patient>> GetFollowUpPatientsAsync(int doctorId);
        IEnumerable<Patient> GetNewPatientsAsync(int doctorId, int daysThreshold = 30);
        Task<List<Patient>> GetActivePatientsAsync(int doctorId);
        Task UpdateImageUrlPatient(string url, int userId);
    }
}