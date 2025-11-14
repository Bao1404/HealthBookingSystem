using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IMedicalHistoriesService
    {
        Task<List<MedicalHistory>> GetHistoryByUserId(int userId);
    }
}
