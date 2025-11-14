using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class MedicalHistoriesRepository : IMedicalHistoriesRepository
    {
        private readonly HealthCareSystemContext _context;
        public MedicalHistoriesRepository(HealthCareSystemContext context)
        {
            _context = context;
        }
        public async Task<List<MedicalHistory>> GetHistoryByUserId(int userId)
        {
            try
            {
                return await _context.MedicalHistories.Where(m => m.PatientUserId == userId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
