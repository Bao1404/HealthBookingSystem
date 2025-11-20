using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Repositories.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly HealthCareSystemContext _context;
        public DoctorRepository(HealthCareSystemContext context)
        {
            _context = context;
        }
        public IEnumerable<Doctor> GetAll() => _context.Doctors.ToList();
        public Doctor GetById(int id) => _context.Doctors.Find(id);
        public void Add(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            _context.SaveChanges();
        }
        public void Update(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var doctor = _context.Doctors.Find(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                _context.SaveChanges();
            }
        }

        public async Task<Doctor> GetDoctorsByIdAsync(int userId)
        {
            try
            {
                return await _context.Doctors.Include(d => d.User).Include(d => d.Specialty).Include(d => d.Appointments).FirstOrDefaultAsync(d => d.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<Doctor>> GetDoctorsAsync()
        {
            return await _context.Doctors.Include(d => d.User)
                                         .Include(d => d.Specialty)
                                         .ToListAsync();
        }

        public IEnumerable<Doctor> GetBySpecialtyAsync(int specialtyId)
        {
            return _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.SpecialtyId == specialtyId);
        }

        public async Task UpdateImageUrlDoctor(string url, int userId)
        {
            try
            {
                var doctor = await _context.Doctors.Include(d => d.User).Include(d => d.Specialty).Include(d => d.Appointments).FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                {
                    doctor.User.AvatarUrl = url;
                    _context.Doctors.Update(doctor);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
