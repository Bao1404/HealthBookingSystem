using BusinessObject.Models;
using Repositories.Interface;
using Repositories.Repositories;
using Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Services.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public IEnumerable<Patient> GetAllPatients() => _patientRepository.GetAll();
        public Patient GetPatientById(int id) => _patientRepository.GetById(id);
        public Patient GetByUserId(int userId) => _patientRepository.GetAll().FirstOrDefault(p => p.UserId == userId);
        public void AddPatient(Patient patient) => _patientRepository.Add(patient);
        public Task UpdatePatient(Patient patient) => _patientRepository.Update(patient);
        public void DeletePatient(int id) => _patientRepository.Delete(id);
        public Task CreatePatient(Patient patient) => _patientRepository.CreatePatient(patient);
        public Task<Patient?> GetByUserIdAsync(int userId) => _patientRepository.GetByUserIdAsync(userId);
        public Task<List<Patient>> GetAllPatientsAsync() => _patientRepository.GetAllPatientsAsync();
        public async Task<List<Patient>> GetPatientsByDoctorAsync(int doctorId)
        {
            return await _patientRepository.GetPatientsByDoctorAsync(doctorId);
        }

        public async Task<Patient?> GetPatientWithDetailsAsync(int userId)
        {
            return await _patientRepository.GetPatientWithDetailsAsync(userId);
        }

        public async Task<List<Patient>> GetPatientsByDoctorAndStatusAsync(int doctorId, string status)
        {
            return await _patientRepository.GetPatientsByDoctorAndStatusAsync(doctorId, status);
        }

        public async Task<List<Patient>> SearchPatientsAsync(int doctorId, string searchTerm)
        {
            return await _patientRepository.SearchPatientsAsync(doctorId, searchTerm);
        }

        public async Task<List<Patient>> GetCriticalPatientsAsync(int doctorId)
        {
            return await _patientRepository.GetCriticalPatientsAsync(doctorId);
        }

        public async Task<List<Patient>> GetFollowUpPatientsAsync(int doctorId)
        {
            return await _patientRepository.GetFollowUpPatientsAsync(doctorId);
        }

        public async Task<List<Patient>> GetNewPatientsAsync(int doctorId, int daysThreshold = 30)
        {
            return await _patientRepository.GetNewPatientsAsync(doctorId, daysThreshold);
        }

        public async Task<List<Patient>> GetActivePatientsAsync(int doctorId)
        {
            return await _patientRepository.GetActivePatientsAsync(doctorId);
        }

        public async Task UpdateImageUrlPatient(string url, int userId)
        {
            await _patientRepository.UpdateImageUrlPatient(url, userId);
        }
    }
}