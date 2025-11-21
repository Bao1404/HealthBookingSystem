using BusinessObject.Models;
using HealthBookingSystem.Models;
using HealthBookingSystemWebApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interface;
using System.Net.Http;
using System.Text.Json;

namespace HealthBookingSystem.Controllers
{
    public class RegisterController : Controller
    {
        private readonly HttpClient _httpClient;
        public RegisterController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
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
            if (!ModelState.IsValid)
            {
                return View("Index", vm);
            }

            var existUser = await CheckExist(vm.Email);
            if (existUser == null)
            {
                var registerUser = new RegisterDTO
                {
                    FullName = vm.FullName,
                    Password = vm.Password,
                    Email = vm.Email,
                    PhoneNumber = vm.PhoneNumber,
                    Dob = vm.Dob,
                    Gender = vm.Gender,
                    Address = vm.Address,
                    BloodType = vm.BloodType,
                    EmergencyContact = vm.EmergencyContact,
                    Weight = vm.Weight,
                    Height = vm.Height,
                    Bmi = vm.Bmi
                };
                
                var response = await _httpClient.PostAsJsonAsync("auth/register-patient", registerUser);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("FinalStep", "Register");
                }
                ViewBag.ErrorMessage = "Error creating account.";
                return View("Index", vm);
            }
            ViewBag.ErrorMessage = "Email already exists.";
            return View("Index", vm);
        }
        private async Task<UserDTO?> CheckExist(string email)
        {
            var url = $"Users/check-exist?email={Uri.EscapeDataString(email)}";
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return null;

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return null;

            return JsonSerializer.Deserialize<UserDTO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
