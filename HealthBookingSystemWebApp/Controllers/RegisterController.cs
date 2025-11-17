using BusinessObject.Models;
using HealthBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interface;

namespace HealthBookingSystem.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;
        public RegisterController(IUserService userService, IPatientService patientService)
        {
            _userService = userService;
            _patientService = patientService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult FinalStep()
        {
            return View();
        }
    }
}
