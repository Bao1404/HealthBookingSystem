using Azure;
using BusinessObject.Models;
using HealthBookingSystem.Models;
using HealthBookingSystemWebApp.DTOs;
using HealthCareSystem.Controllers.dto;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Service;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using static Google.Apis.Requests.BatchRequest;

namespace HealthCareSystem.Controllers
{
    public class AdminController : Controller
    {

        private readonly HttpClient _client;
        private int? currentUser => HttpContext.Session.GetInt32("AccountId");
        private string? currentRole => HttpContext.Session.GetString("Role");

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("APIClient");
        }
        public async Task<IActionResult> ManageDoctors()
        {
            ViewData["ActiveMenu"] = "ManageDoctors";
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (currentRole == "Doctor")
            {
                return RedirectToAction("Index", "Doctor");
            }
            if (currentRole == "Patient")
            {
                return RedirectToAction("Index", "User");
            }
            var doctors = await GetAllDoctors();
            ViewBag.Specialties = await GetAllSpecialties();
            return View(doctors);
        }
        [HttpPost]
        public async Task<IActionResult> AddDoctor(IFormCollection form)
        {
            var fullName = form["fullName"];
            var email = form["email"];
            var phoneNumber = form["phone"];
            var password = form["password"];
            var confirmPassword = form["confirmPassword"];
            var experience = form["exp"];
            var bio = form["bio"];
            var specialtyId = form["specialty"];
            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToAction("ManageDoctors");
            }
            var doctor = new RegisterDoctorDTO
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Password = password,
                Experience = experience,
                Bio = bio,
                SpecialtyId = int.Parse(specialtyId)
            };
            var response = await _client.PostAsJsonAsync("auth/register-doctor", doctor);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Doctor added successfully.";
                TempData["Type"] = "success";
                return RedirectToAction("ManageDoctors");
            }
            else
            {
                TempData["Message"] = "Fail to add new doctor.";
                TempData["Type"] = "error";
                return RedirectToAction("ManageDoctors");
            }
        }

        public async Task<IActionResult> ManagePatients()
        {
            ViewData["ActiveMenu"] = "ManagePatients";
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (currentRole == "Doctor")
            {
                return RedirectToAction("Index", "Doctor");
            }
            if (currentRole == "Patient")
            {
                return RedirectToAction("Index", "User");
            }
            var patients = await GetAllPatients();
            return View(patients);
        }
        private async Task<List<PatientDTO>> GetAllPatients()
        {
            var request = await _client.GetAsync("patients?$expand=User");
            if (request.IsSuccessStatusCode)
            {
                var patients = await request.Content.ReadFromJsonAsync<List<PatientDTO>>();
                return patients ?? new List<PatientDTO>();
            }
            return new List<PatientDTO>();
        }
        public async Task<IActionResult> DoctorDetail(int id)
        {
            try
            {
                var doctor = await GetDoctorsById(id);
                return View(doctor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        private async Task<DoctorDTO> GetDoctorsById(int doctorId)
        {
            var request = await _client.GetAsync($"Doctors/{doctorId}?$expand=Appointments,User,Specialty,TimeOffs,WorkingHours");
            if (request.IsSuccessStatusCode)
            {
                var doctor = await request.Content.ReadFromJsonAsync<DoctorDTO>();
                return doctor ?? new DoctorDTO();
            }
            return new DoctorDTO();
        }
        // User Detail Pages
        public async Task<IActionResult> PatientDetail(int id)
        {
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (currentRole == "Doctor")
            {
                return RedirectToAction("Index", "Doctor");
            }
            if (currentRole == "Patient")
            {
                return RedirectToAction("Index", "User");
            }
            var patient = await GetPatientDetail(id);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Patients");
            }
            var viewModel = await BuildPatientDetailsViewModelAsync(patient, currentUser.Value);
            return View(viewModel);
        }
        private async Task<List<DoctorDTO>> GetAllDoctors()
        {
            var request = await _client.GetAsync("doctors?$expand=Appointments,User,Specialty,TimeOffs,WorkingHours");
            if (request.IsSuccessStatusCode)
            {
                var doctors = await request.Content.ReadFromJsonAsync<List<DoctorDTO>>();
                return doctors ?? new List<DoctorDTO>();
            }
            return new List<DoctorDTO>();
        }
        private async Task<List<SpecialtyDTO>> GetAllSpecialties()
        {
            var request = await _client.GetAsync("Specialties?$expand=Doctors");
            if (request.IsSuccessStatusCode)
            {
                var specialties = await request.Content.ReadFromJsonAsync<List<SpecialtyDTO>>();
                return specialties ?? new List<SpecialtyDTO>();
            }
            return new List<SpecialtyDTO>();

        }
        private async Task<PatientDTO> GetPatientDetail(int id)
        {
            var request = await _client.GetAsync($"patients/{id}?$expand=User,Appointments($expand=DoctorUser($expand=User),MedicalRecords),MedicalHistories");
            if (request.IsSuccessStatusCode)
            {
                var patient = await request.Content.ReadFromJsonAsync<PatientDTO>();
                return patient ?? new PatientDTO();
            }
            return new PatientDTO();
        }
        public async Task<IActionResult> UserDelete(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var response = await _client.GetAsync($"User/{id}");
                if (!response.IsSuccessStatusCode)
                    return BadRequest("API Error: " + response.StatusCode);

                var user = await response.Content.ReadFromJsonAsync<User>();
                if (user == null)
                {
                    return NotFound("User not found");
                }
                return View(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        private async Task<List<AppointmentDTO>> GetAppointmentsUserId(int userId)
        {
            var request = await _client.GetAsync($"appointments/user/{userId}?$expand=PatientUser($expand=User),DoctorUser($expand=User),MedicalRecords");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        private async Task<PatientDetailsViewModel> BuildPatientDetailsViewModelAsync(PatientDTO patient, int doctorId)
        {
            var appointments = await GetAppointmentsUserId(patient.UserId);

            var appointmentHistory = appointments.Select(a => new AppointmentHistory
            {
                AppointmentId = a.AppointmentId,
                AppointmentDateTime = a.AppointmentDateTime,
                AppointmentDoctor = a.DoctorUser?.User.FullName ?? "Unknown",
                Status = a.Status ?? "Unknown",
                Notes = a.Notes ?? "",
                DoctorNotes = a.Notes ?? "",
                CreatedAt = a.CreatedAt
            }).ToList();

            var statistics = new PatientStatistics
            {
                TotalAppointments = appointments.Count,
                CompletedAppointments = appointments.Count(a => a.Status == "Completed"),
                CancelledAppointments = appointments.Count(a => a.Status == "Cancelled"),
                FirstAppointment = appointments.LastOrDefault()?.AppointmentDateTime,
                LastAppointment = appointments.FirstOrDefault()?.AppointmentDateTime,
                PatientSince = patient.CreatedAt.ToString("MMMM yyyy")
            };

            return new PatientDetailsViewModel
            {
                Patient = MapToPatientInfo(patient),
                AppointmentHistory = appointmentHistory,
                MedicalRecords = patient.Appointments
                    .SelectMany(a => a.MedicalRecords)
                    .OrderByDescending(mr => mr.CreatedAt)
                    .ToList(),
                Statistics = statistics
            };
        }
        private PatientInfo MapToPatientInfo(PatientDTO patient)
        {
            var doctorAppointments = patient.Appointments.OrderBy(a => a.AppointmentDateTime).ToList();
            var lastAppointment = doctorAppointments.LastOrDefault(a => a.AppointmentDateTime <= DateTime.Now);
            var nextAppointment = doctorAppointments.FirstOrDefault(a => a.AppointmentDateTime > DateTime.Now && a.Status != "Cancelled");

            var age = patient.DateOfBirth.HasValue
                ? DateTime.Now.Year - patient.DateOfBirth.Value.Year
                : (int?)null;

            return new PatientInfo
            {
                UserId = patient.UserId,
                FullName = patient.User.FullName,
                Email = patient.User.Email,
                Phone = patient.User.PhoneNumber,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Age = age,
                BloodType = patient.BloodType,
                Allergies = patient.Allergies,
                Weight = patient.Weight,
                Height = patient.Height,
                Bmi = patient.Bmi,
                Address = patient.Address,
                EmergencyPhoneNumber = patient.EmergencyPhoneNumber,
                AvatarUrl = patient.User.AvatarUrl ?? "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg",
                LastAppointment = lastAppointment?.AppointmentDateTime,
                NextAppointment = nextAppointment?.AppointmentDateTime,
                TotalAppointments = doctorAppointments.Count,
                CompletedAppointments = doctorAppointments.Count(a => a.Status == "Completed"),
                CreatedAt = patient.CreatedAt,
                CreatedAtDisplay = patient.CreatedAt.ToString("MMM dd, yyyy"),
                RecentAppointments = doctorAppointments.TakeLast(3).Select(a => new RecentAppointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = a.Status ?? "Unknown",
                    Notes = a.Notes ?? "",
                    DateDisplay = a.AppointmentDateTime.ToString("MMM dd"),
                    TimeDisplay = a.AppointmentDateTime.ToString("HH:mm")
                }).ToList()
            };
        }
    }
}