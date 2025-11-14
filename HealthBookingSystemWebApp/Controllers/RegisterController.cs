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
        [HttpPost("/Register")]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            var name = vm.FullName;
            var phone = vm.PhoneNumber;
            var aller = vm.Allergies;
            if (!ModelState.IsValid)
            {
                return View("Index", vm);
            }

            var existUser = await _userService.CheckUserExist(vm.Email);
            if (existUser == null)
            {
                var user = new User
                {
                    Email = vm.Email,
                    Password = vm.Password,
                    FullName = vm.FullName,
                    PhoneNumber = vm.PhoneNumber,
                    Role = vm.Role,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    AvatarUrl = "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg"
                };

                await _userService.CreateUser(user);

                if (user.Role.ToLower().Equals("patient"))
                {
                    user.Patient = new Patient
                    {
                        UserId = user.UserId,
                        DateOfBirth = vm.Dob,
                        Gender = vm.Gender,
                        Address = vm.Address,
                        BloodType = vm.BloodType,
                        EmergencyPhoneNumber = vm.EmergencyContact,
                        Weight = vm.Weight,
                        Height = vm.Height,
                        Bmi = vm.Bmi,
                        Allergies = vm.Allergies,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _patientService.AddPatient(user.Patient);
                }
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return RedirectToAction("FinalStep", "Register");
            }
            ViewBag.ErrorMessage = "Email already exists.";
            return View("Index", vm);
        }
    }
}
