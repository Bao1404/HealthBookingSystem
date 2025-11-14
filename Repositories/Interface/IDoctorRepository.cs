using BusinessObject.Models;
using System.Collections.Generic;

namespace Repositories.Interface
{
    public interface IDoctorRepository
    {
        IEnumerable<Doctor> GetAll();
        Doctor GetById(int id);
        void Add(Doctor doctor);
        void Update(Doctor doctor);
        void Delete(int id);
        public Task<Doctor> GetDoctorsByIdAsync(int id);

        Task<List<Doctor>> GetDoctorsAsync();
        Task<List<Doctor>> GetBySpecialtyAsync(int specialtyId);
        Task UpdateImageUrlDoctor(string url, int userId);
    }
}
