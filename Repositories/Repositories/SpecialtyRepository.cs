using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Repositories
{
    public class SpecialtyRepository : ISpecialtyRepository
    {
        private readonly HealthCareSystemContext _context;
        public SpecialtyRepository(HealthCareSystemContext context)
        {
            _context = context;
        }
        public async Task<List<Specialty>> GetAllSpecialtiesAsync()
        {
            return await _context.Specialties.ToListAsync();
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
