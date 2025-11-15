using HealthBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace HealthBookingSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        public LoginController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("/Login")]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", vm);
            }

            string email = vm.Email;
            string password = vm.Password;

            var account = await _userService.Login(email, password);
            if (account != null)
            {
                HttpContext.Session.SetInt32("UserId", account.UserId);

                if (account.Role.Equals("Patient"))
                {
                    return RedirectToAction("Index", "User");
                }
                if (account.Role.Equals("Doctor"))
                {
                    return RedirectToAction("Index", "Doctor");
                }
                if (account.Role.Equals("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            ViewBag.ErrorMessage = "Invalid email or password.";
            return View("Index", vm);
        }
    }
}
