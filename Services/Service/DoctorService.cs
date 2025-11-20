using BusinessObject.Models;
using Repositories.Interface;
using Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Services.Service
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        public DoctorService(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }
        public IEnumerable<Doctor> GetAllDoctors() => _doctorRepository.GetAll();
        public Doctor GetDoctorById(int id) => _doctorRepository.GetById(id);
        public void AddDoctor(Doctor doctor) => _doctorRepository.Add(doctor);
        public void UpdateDoctor(Doctor doctor) => _doctorRepository.Update(doctor);
        public void DeleteDoctor(int id) => _doctorRepository.Delete(id);
        public IEnumerable<Doctor> GetBySpecialty(int specialtyId) => _doctorRepository.GetAll().Where(d => d.SpecialtyId == specialtyId);

        public Task<Doctor> GetDoctorsByIdAsync(int id)
        {
            return _doctorRepository.GetDoctorsByIdAsync(id);
        }
        public async Task<List<Doctor>> GetDoctorsAsync() => 
            await _doctorRepository.GetDoctorsAsync();
        public  IEnumerable<Doctor> GetBySpecialtyAsync(int specialtyId) => 
             _doctorRepository.GetBySpecialtyAsync(specialtyId);
        public async Task UpdateImageUrlDoctor(string url, int userId)
        {
             await _doctorRepository.UpdateImageUrlDoctor(url, userId);
        }
    }
}
