using HealthBookingSystem.Models;
using HealthBookingSystemWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace HealthBookingSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", vm);
            }

            var response = await _httpClient.PostAsJsonAsync("Auth/login", vm);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountViewModel>();

                if (result != null)
                {
                    HttpContext.Session.SetString("Token", result.Token);
                    HttpContext.Session.SetString("Role", result.Role);
                    HttpContext.Session.SetInt32("AccountId", result.AccountId);
                    if (result.Role.Equals("Patient"))
                    {
                        return RedirectToAction("Index", "User");
                    }
                    else if (result.Role.Equals("Doctor"))
                    {
                        return RedirectToAction("Index", "Doctor");
                    }
                    else if(result.Role.Equals("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }
            }
            return View();
        }
    }
}
