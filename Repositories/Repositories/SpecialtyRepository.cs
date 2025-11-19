using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System.Collections;

namespace Repositories.Repositories
{
    public class SpecialtyRepository : ISpecialtyRepository
    {
        private readonly HealthCareSystemContext _context;
        public SpecialtyRepository(HealthCareSystemContext context)
        {
            _context = context;
        }
        public IEnumerable<Specialty> GetAllSpecialtiesAsync()
        {
            return _context.Specialties;
        }
        public async Task<Specialty> GetSpecialtyByName(string name)
        {
            try
            {
                return await _context.Specialties.FirstOrDefaultAsync(s => s.Name.Equals(name));
            }
            catch(Exception ex)
            {
                throw new Exception($"Error retrieving specialty by name: {name}", ex);
            }
        }
    }
}
