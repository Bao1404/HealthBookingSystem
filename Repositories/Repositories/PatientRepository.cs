using BusinessObject.Models;
using Repositories.Interface;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace Repositories.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly HealthCareSystemContext _context;
        public PatientRepository(HealthCareSystemContext context)
        {
            _context = context;
        }
        public IEnumerable<Patient> GetAll() => _context.Patients.ToList();
        public Patient GetById(int id) => _context.Patients.Include(p => p.User).FirstOrDefault( p=> p.UserId == id);
        public void Add(Patient patient)
        {
            _context.Patients.Add(patient);
            _context.SaveChanges();
        }
        public async Task Update(Patient patient)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }
        public void Delete(int id)
        {
            var patient = _context.Patients.Find(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                _context.SaveChanges();
            }
        }
        public async Task CreatePatient(Patient patient)
        {
            try
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Patient?> GetByUserIdAsync(int userId)
        {
            return await _context.Patients.Include(p => p.User).Include(p => p.MedicalHistories).FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            try
            {
                return await _context.Patients.Include(d => d.User).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public IEnumerable<Patient> GetPatientsByDoctorAsync(int doctorId)
        {
            return _context.Patients
                .Where(p => p.Appointments.Any(a => a.DoctorUserId == doctorId))
                .Distinct()
                .OrderBy(p => p.User.FullName);
        }

        public async Task<Patient?> GetPatientWithDetailsAsync(int userId)
        {
            return await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.DoctorUser)
                        .ThenInclude(d => d.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.MedicalRecords)
                .Include(p => p.MedicalHistories)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<List<Patient>> GetPatientsByDoctorAndStatusAsync(int doctorId, string status)
        {
            return await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.DoctorUser)
                .Where(p => p.Appointments.Any(a => a.DoctorUserId == doctorId && a.Status == status))
                .Distinct()
                .OrderBy(p => p.User.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> SearchPatientsAsync(int doctorId, string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.DoctorUser)
                .Where(p => p.Appointments.Any(a => a.DoctorUserId == doctorId) &&
                           (p.User.FullName.ToLower().Contains(lowerSearchTerm) ||
                            p.User.Email.ToLower().Contains(lowerSearchTerm) ||
                            (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(searchTerm))))
                .Distinct()
                .OrderBy(p => p.User.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetCriticalPatientsAsync(int doctorId)
        {
            // Define critical patients as those with recent emergency appointments or specific conditions
            return await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.DoctorUser)
                .Where(p => p.Appointments.Any(a => a.DoctorUserId == doctorId &&
                           (a.Notes != null && a.Notes.ToLower().Contains("emergency")) ||
                           (a.Notes != null && a.Notes.ToLower().Contains("urgent")) ||
                           (a.Notes != null && a.Notes.ToLower().Contains("critical"))))
                .Distinct()
                .OrderBy(p => p.User.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetFollowUpPatientsAsync(int doctorId)
        {
            // Patients who need follow-up (completed appointments in last 30 days that mentioned follow-up)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            return await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.DoctorUser)
                .Where(p => p.Appointments.Any(a => a.DoctorUserId == doctorId &&
                           a.Status == "Completed" &&
                           a.AppointmentDateTime >= thirtyDaysAgo &&
                           (a.Notes != null && a.Notes.ToLower().Contains("follow-up"))))
                .Distinct()
                .OrderBy(p => p.User.FullName)
                .ToListAsync();
        }

        public IEnumerable<Patient> GetNewPatientsAsync(int doctorId, int daysThreshold = 120)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysThreshold);
            return _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.DoctorUser)
                .Where(p => p.Appointments.Any(a => a.DoctorUserId == doctorId) &&
                           p.CreatedAt >= cutoffDate)
                .Distinct()
                .OrderByDescending(p => p.CreatedAt);
        }

        public async Task<List<Patient>> GetActivePatientsAsync(int doctorId)
        {
            // Patients with confirmed or completed appointments in the last 90 days
            var ninetyDaysAgo = DateTime.Now.AddDays(-90);
            return await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.DoctorUser)
                .Where(p => p.Appointments.Any(a => a.DoctorUserId == doctorId &&
                           (a.Status == "Confirmed" || a.Status == "Completed") &&
                           a.AppointmentDateTime >= ninetyDaysAgo))
                .Distinct()
                .OrderBy(p => p.User.FullName)
                .ToListAsync();
        }

        public async Task UpdateImageUrlPatient(string url, int userId)
        {
            try
            {
                var patient = await _context.Patients.Include(d => d.User).FirstOrDefaultAsync(d => d.UserId == userId);
                if (patient != null)
                {
                    patient.User.AvatarUrl = url;
                    _context.Patients.Update(patient);
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
