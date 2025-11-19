using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface ISpecialtyRepository
    {
        IEnumerable<Specialty> GetAllSpecialtiesAsync();
        Task<Specialty> GetSpecialtyByName(string name);
    }
}
