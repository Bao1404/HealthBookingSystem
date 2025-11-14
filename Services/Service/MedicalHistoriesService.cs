using BusinessObject.Models;
using Repositories.Interface;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class MedicalHistoriesService : IMedicalHistoriesService
    {
        private readonly IMedicalHistoriesRepository _medicalHistoriesRepository;
        public MedicalHistoriesService(IMedicalHistoriesRepository medicalHistoriesRepository)
        {
            _medicalHistoriesRepository = medicalHistoriesRepository;
        }
        public Task<List<MedicalHistory>> GetHistoryByUserId(int userId) => _medicalHistoriesRepository.GetHistoryByUserId(userId);
    }
}
