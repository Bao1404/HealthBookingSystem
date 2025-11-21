using BusinessObject.Models;
using HealthBookingSystem.Models;
using HealthBookingSystem.Service;
using HealthBookingSystemWebApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Repositories.IRepositories;
using Services;
using Services.Interface;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HealthBookingSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private int? currentUser => HttpContext.Session.GetInt32("AccountId");
        private readonly PhotoService _photoService;
        private readonly IMedicalHistoriesService _medicalHistoriesService;

        public UserController(PhotoService photoService, IHttpClientFactory httpClientFactory, IMedicalHistoriesService medicalHistoriesService)
        {
            _photoService = photoService;
            _httpClient = httpClientFactory.CreateClient("APIClient");
            _medicalHistoriesService = medicalHistoriesService;
        }
        public async Task<IActionResult> Index()
        {
            ViewData["ActiveMenu"] = "Dashboard";
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await GetUserById(currentUser.Value);
            var allAppointments = await GetAppointmentByUserId(currentUser.Value);
            // Filter appointments for current user
            var userAppointments = allAppointments.Where(a => a.PatientUserId == currentUser.Value).ToList();

            var dashboardModel = new DashboardViewModel
            {
                CurrentUser = user,
                UpcomingAppointments = await GetUpcomingAppointments(currentUser.Value),
                RecentAppointments = await GetRecentAppointments(currentUser.Value),
                TodayAppointments = await GetTodayAppointments(currentUser.Value),
                Stats = await CalculateStats(userAppointments)
            };

            return View("Index", dashboardModel);
        }


        private async Task<UserDTO> GetUserById(int id)
        {
            var request = await _httpClient.GetAsync($"Users/{id}");
            if (request.IsSuccessStatusCode)
            {
                var user = await request.Content.ReadFromJsonAsync<UserDTO>();
                return user ?? new UserDTO();
            }
            return new UserDTO();
        }
        private async Task<List<AppointmentDTO>> GetUpcomingAppointments(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Appointments/user/{doctorId}?&$expand=DoctorUser($expand=User,Specialty),MedicalRecords,PatientUser($expand=User)&$filter=Status eq 'Upcoming'&$orderby=AppointmentDateTime asc");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }

        private async Task<List<AppointmentViewModel>> GetRecentAppointments(int userId)
        {
            var request = await _httpClient.GetAsync($"Appointments/user/{userId}?&$expand=DoctorUser($expand=User,Specialty),MedicalRecords,PatientUser($expand=User)");
            var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
            var viewModels = appointments.Select(a => new AppointmentViewModel
            {
                AppointmentId = a.AppointmentId,
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status ?? "Unknown",
                Notes = a.Notes ?? "",
                DoctorName = a.DoctorUser?.User?.FullName ?? "Unknown Doctor",
                SpecialtyName = a.DoctorUser?.Specialty?.Name ?? "General",
                PatientName = a.PatientUser?.User?.FullName ?? "Unknown Patient",
                DoctorAvatarUrl = a.DoctorUser?.User?.AvatarUrl ?? "/images/default-doctor.png",
                CreatedAt = a.CreatedAt
            }).ToList();

            return viewModels;
        }

        private async Task<List<AppointmentDTO>> GetTodayAppointments(int doctorId)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var filter = $"date(AppointmentDateTime) eq {today}&$orderby=AppointmentDateTime asc";
            var request = await _httpClient.GetAsync($"Appointments/user/{doctorId}?$expand=DoctorUser($expand=User,Specialty),MedicalRecords,PatientUser($expand=User)&filter={filter}");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }

        private async Task<DashboardStats> CalculateStats(List<AppointmentDTO> appointments)
        {
            var now = DateTime.Now;
            var weekStart = now.AddDays(-(int)now.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            return new DashboardStats
            {
                UpcomingAppointmentCount = appointments.Count(a => a.AppointmentDateTime > now && (a.Status == "Pending" || a.Status == "Confirmed")),
                CompletedVisitCount = appointments.Count(a => a.AppointmentDateTime <= now && a.Status == "Completed"),
                UnreadMessageCount = 2, // This would come from messages service when implemented
                HealthScore = 85,
                ThisWeekAppointmentCount = appointments.Count(a =>
                    a.AppointmentDateTime >= weekStart &&
                    a.AppointmentDateTime < weekEnd &&
                    (a.Status == "Upcoming")),
                ThisWeekCheckupCount = appointments.Count(a =>
                    a.AppointmentDateTime >= weekStart &&
                    a.AppointmentDateTime < weekEnd &&
                    a.Notes != null && a.Notes.ToLower().Contains("checkup")),
                ThisWeekFollowupCount = appointments.Count(a =>
                    a.AppointmentDateTime >= weekStart &&
                    a.AppointmentDateTime < weekEnd &&
                    a.Notes != null && a.Notes.ToLower().Contains("follow"))
            };
        }
        private async Task<List<AppointmentDTO>> GetAppointmentByUserId(int userId)
        {
            var request = await _httpClient.GetAsync($"Appointments/user/{userId}?$expand=DoctorUser($expand=User),MedicalRecords,PatientUser($expand=User)");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        public async Task<IActionResult> Appointments()
        {
            ViewData["ActiveMenu"] = "Appointments";
            
            var user = await GetUserById(currentUser.Value);
            ViewBag.CurrentUser = user;
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var allAppointments = await GetAppointmentByUserId(currentUser.Value);
            var userAppointments = allAppointments.Where(a => a.PatientUserId == currentUser.Value).ToList();

            var model = new BookAppointmentViewModel();
            model.Specialties = (await GetAllSpecialties())
                .Select(s => new SpecialtyViewModel
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();


            // Add appointment data to ViewBag for the appointments list
            ViewBag.UpcomingAppointments = GetUpcomingAppointments(currentUser.Value);

            ViewBag.PastAppointments = userAppointments
                .Where(a => a.AppointmentDateTime <= DateTime.Now)
                .OrderByDescending(a => a.AppointmentDateTime)
                .Select(a => new AppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = a.Status ?? "Unknown",
                    Notes = a.Notes ?? "",
                    DoctorName = a.DoctorUser?.User?.FullName ?? "Unknown Doctor",
                    SpecialtyName = a.DoctorUser?.Specialty?.Name ?? "General",
                    PatientName = a.PatientUser?.User?.FullName ?? "Unknown Patient",
                    DoctorAvatarUrl = a.DoctorUser?.User?.AvatarUrl ?? "/images/default-doctor.png",
                    CreatedAt = a.CreatedAt
                }).ToList();
            ViewBag.CancelledAppointments = userAppointments
                .Where(a => a.Status == "Cancelled")
                .OrderByDescending(a => a.AppointmentDateTime)
                .Select(a => new AppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = a.Status ?? "Unknown",
                    Notes = a.Notes ?? "",
                    DoctorName = a.DoctorUser?.User?.FullName ?? "Unknown Doctor",
                    SpecialtyName = a.DoctorUser?.Specialty?.Name ?? "General",
                    PatientName = a.PatientUser?.User?.FullName ?? "Unknown Patient",
                    DoctorAvatarUrl = a.DoctorUser?.User?.AvatarUrl ?? "/images/default-doctor.png",
                    CreatedAt = a.CreatedAt
                }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Calendar()
        {
            ViewData["ActiveMenu"] = "Calendar";

            var user = await GetUserById(currentUser.Value);

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.CurrentUser = user;

            var allAppointments = await _httpClient.GetFromJsonAsync<List<AppointmentDTO>>("Appointments");
            var userAppointments = allAppointments.Where(a => a.PatientUserId == currentUser.Value).ToList();

            var model = new BookAppointmentViewModel();
            model.Specialties = (await GetAllSpecialties())
                .Select(s => new SpecialtyViewModel
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();

            //Prepare calendar data
            ViewBag.TodayAppointments = await GetTodayAppointments(currentUser.Value);
            ViewBag.WeekAppointments = await GetWeekAppointments(currentUser.Value);
            ViewBag.MonthAppointments = await GetMonthAppointments(currentUser.Value);
            ViewBag.CurrentDate = DateTime.Now;

            return View(model);
        }

        private async Task<List<AppointmentDTO>> GetWeekAppointments(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Appointments/week/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }

        private async Task<List<AppointmentDTO>> GetMonthAppointments(int doctorId)
        {
            var request = await _httpClient.GetAsync($"appointments/month/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }

        public async Task<IActionResult> Doctors()
        {
            ViewData["ActiveMenu"] = "Doctors";
            if (currentUser.Value == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await GetUserById(currentUser.Value);

            ViewBag.CurrentUser = user;

            var doctors =  await GetAllDoctor();
            var specialties = await GetAllSpecialties();

            ViewBag.Doctors = doctors;
            ViewBag.Specialties = specialties;

            return View(user);
        }
        private async Task<List<DoctorDTO>> GetAllDoctor()
        {
            var request = await _httpClient.GetAsync("Doctors?$expand=User,Specialty");
            if (request.IsSuccessStatusCode)
            {
                var doctors = await request.Content.ReadFromJsonAsync<List<DoctorDTO>>();
                return doctors ?? new List<DoctorDTO>();
            }
            return new List<DoctorDTO>();
        }
        public async Task<IActionResult> Messages(int conversationId)
        {

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login"); 
            }
            var conversation = await _httpClient.GetFromJsonAsync<List<ConversationDto>>($"ApiConversation/GetConversationsByPatient/{currentUser.Value}");

            if (conversation == null)
            {
                return RedirectToAction("Index", "Home"); 
            }
            var patient = await _httpClient.GetFromJsonAsync<PatientDTO>($"patients/{currentUser.Value}?$expand=User,Appointments($expand=DoctorUser($expand=User),MedicalRecords),MedicalHistories");

            ViewData["PatientId"] = currentUser.Value;
            ViewData["ConversationId"] = conversationId; 
            ViewData["ActiveMenu"] = "Messages";
            return View(patient);
        }

        public async Task<IActionResult> ChatBox()
        {
            if(currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var user = await GetUserById(currentUser.Value);
            ViewData["ActiveMenu"] = "ChatBox";
            return View(user);
        }
        public async Task<IActionResult> Profile()
        {
            ViewData["ActiveMenu"] = "Profile";
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var patient = await _httpClient.GetFromJsonAsync<PatientDTO>($"Patients/{currentUser.Value}?$expand=User");

            //ViewBag.MedicalHistory = await _httpClient.GetFromJsonAsync<List<MedicalHistory>>($"MedicalHistories/{currentUser.Value}");

            ViewBag.MedicalHistory = await _medicalHistoriesService.GetHistoryByUserId(currentUser.Value);
            return View(patient);
        }
        private async Task<List<SpecialtyDTO>> GetAllSpecialties()
        {
            var request = await _httpClient.GetAsync("Specialties?$expand=Doctors");
            if (request.IsSuccessStatusCode)
            {
                var specialties = await request.Content.ReadFromJsonAsync<List<SpecialtyDTO>>();
                return specialties ?? new List<SpecialtyDTO>();
            }
            return new List<SpecialtyDTO>();

        }
        [HttpPost("Edit")]
        public async Task<IActionResult> UpdateProfile(IFormCollection form)
        {
            var userId = Request.Form["userId"];
            var email = Request.Form["email"];
            var fullName = Request.Form["fullName"];
            var phoneNumber = Request.Form["phone"];
            var dateOfBirth = Request.Form["dob"];
            var address = Request.Form["address"];
            var gender = Request.Form["gender"];
            var emergencyPhoneNumber = Request.Form["ePhone"];

            var patientUpdate = new PatientUpdateDTO
            {
                Email = email,
                FullName = fullName,
                Phone = phoneNumber,
                DateOfBirth = DateOnly.Parse(dateOfBirth),
                Address = address,
                Gender = gender,
                EmergencyPhoneNumber = emergencyPhoneNumber
            };

            await _httpClient.PutAsJsonAsync($"Patients/{userId}", patientUpdate);

            return RedirectToAction("Profile", "User");

        }
        [HttpPost("Health")]
        public async Task<IActionResult> UpdateHealthProfile()
        {
            var userId = Request.Form["userId"];
            var height = Request.Form["height"];
            var weight = Request.Form["weight"];
            var bloodType = Request.Form["blood"];
            var heightM = double.Parse(height) / 100.0;
            double bmi = double.Parse(weight) / (heightM * heightM);

            var patientUpdate = new PatientUpdateDTO
            {
                Height = int.Parse(height),
                Weight = int.Parse(weight),
                BloodType = bloodType,
                Bmi = (decimal)bmi
            };

            await _httpClient.PutAsJsonAsync($"Patients/{userId}", patientUpdate);

            return RedirectToAction("Profile", "User");
        }
        // API Methods for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetDoctorsBySpecialty(int specialtyId)
        {
            // Gọi hàm async nếu có
            var doctors = await _httpClient.GetFromJsonAsync<List<DoctorDTO>>($"Doctors?&$expand=User,Specialty&$filter=SpecialtyId eq {specialtyId}");

            var doctorViewModels = doctors.Select(d => new DoctorViewModel
            {
                UserId = d.UserId,
                FullName = d.User?.FullName ?? "Unknown",
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty?.Name ?? "Unknown",
                Qualifications = d.Qualifications,
                Experience = d.Experience,
                Rating = d.Rating,
                AvatarUrl = d.User?.AvatarUrl ?? "/images/default-doctor.png"
            }).ToList();

            return PartialView("_DoctorOptions", doctorViewModels);
        }
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            var timeSlots = new List<TimeSlotViewModel>();
            var workingHours = new[]
            {
  
                new TimeSpan(9, 0, 0),   // 9:00 AM
                new TimeSpan(9, 30, 0),  // 9:30 AM
                new TimeSpan(10, 0, 0),  // 10:00 AM
                new TimeSpan(10, 30, 0), // 10:30 AM
                new TimeSpan(11, 0, 0),  // 11:00 AM
                new TimeSpan(11, 30, 0), // 11:30 AM
                new TimeSpan(14, 0, 0),  // 2:00 PM
                new TimeSpan(14, 30, 0), // 2:30 PM
                new TimeSpan(15, 0, 0),  // 3:00 PM
                new TimeSpan(15, 30, 0), // 3:30 PM
                new TimeSpan(16, 0, 0),  // 4:00 PM
                new TimeSpan(16, 30, 0), // 4:30 PM

            };

            foreach (var time in workingHours)
            {
                var appointmentDateTime = date.Add(time);
                var isBooked = (await _httpClient.PostAsJsonAsync("Apppointments/GetAvailableTimeSlots", new { doctorId, appointmentDateTime } )).IsSuccessStatusCode;

                timeSlots.Add(new TimeSlotViewModel
                {
                    Time = time,
                    IsAvailable = !isBooked && appointmentDateTime > DateTime.Now,
                    DisplayTime = $"{time.Hours:00}:{time.Minutes:00}"
                });
            }

            return PartialView("_TimeSlotOptions", timeSlots);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlotsForReschedule(int doctorId, string date, int excludeAppointmentId)
        {
            // Parse date
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format");
            }

            var timeSlots = new List<TimeSlotViewModel>();
            var workingHours = new[]
            {
                new TimeSpan(9, 0, 0),   // 9:00 AM
                new TimeSpan(9, 30, 0),  // 9:30 AM
                new TimeSpan(10, 0, 0),  // 10:00 AM
                new TimeSpan(10, 30, 0), // 10:30 AM
                new TimeSpan(11, 0, 0),  // 11:00 AM
                new TimeSpan(11, 30, 0), // 11:30 AM
                new TimeSpan(14, 0, 0),  // 2:00 PM
                new TimeSpan(14, 30, 0), // 2:30 PM
                new TimeSpan(15, 0, 0),  // 3:00 PM
                new TimeSpan(15, 30, 0), // 3:30 PM
                new TimeSpan(16, 0, 0),  // 4:00 PM
                new TimeSpan(16, 30, 0), // 4:30 PM
            };

            foreach (var time in workingHours)
            {
                var appointmentDateTime = parsedDate.Add(time);
                var isBooked = (await _httpClient.PostAsJsonAsync("Apppointments/GetAvailableTimeSlotsForReschedule", new { doctorId, appointmentDateTime, excludeAppointmentId })).IsSuccessStatusCode;

                timeSlots.Add(new TimeSlotViewModel
                {
                    Time = time,
                    IsAvailable = !isBooked && appointmentDateTime > DateTime.Now,
                    DisplayTime = $"{time.Hours:00}:{time.Minutes:00}"
                });
            }

            return PartialView("_TimeSlotOptions", timeSlots);
        }
        private async Task<DoctorDTO> GetDoctorsById(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Doctors/{doctorId}?$expand=Appointments,User,Specialty,TimeOffs,WorkingHours");
            if (request.IsSuccessStatusCode)
            {
                var doctor = await request.Content.ReadFromJsonAsync<DoctorDTO>();
                return doctor ?? new DoctorDTO();
            }
            return new DoctorDTO();
        }
        [HttpPost]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            if (currentUser.Value == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                // Reload data for form
                model.Specialties = (await _httpClient.GetFromJsonAsync<List<SpecialtyDTO>>("Specialties?$expand=Doctors"))
                    .Select(s => new SpecialtyViewModel
                    {
                        SpecialtyId = s.SpecialtyId,
                        Name = s.Name,
                        Description = s.Description
                    }).ToList();

                TempData["Error"] = "Please fill in all required information";
                return View("Appointments", model);
            }

            try
            {
                // Validate that current user exists as a patient
                // Remove or comment out the following usages:
                // var patient = await _patientService.GetByUserIdAsync(currentUserId.Value);
                // If you need this feature, implement a synchronous version in the service and repository, otherwise remove the related usages.
                var patient = await GetUserById(currentUser.Value);
                if (patient == null)
                {
                    TempData["Error"] = "Patient record not found. Please contact support.";
                    return RedirectToAction("Appointments");
                }

                // Validate that selected doctor exists
                DoctorDTO doctor = null;
                try
                {
                    doctor = await GetDoctorsById(model.DoctorUserId);
                }
                catch (Exception)
                {
                    // Doctor not found or error occurred
                }

                if (doctor == null)
                {
                    TempData["Error"] = "Selected doctor not found. Please choose another doctor.";
                    return RedirectToAction("Appointments");
                }

                var appointmentDateTime = model.AppointmentDate.Add(model.AppointmentTime);

                // Check if time slot is still available
                var isBooked = (await _httpClient.PostAsJsonAsync("Apppointments/GetAvailableTimeSlots", new { model.DoctorUserId, appointmentDateTime })).IsSuccessStatusCode;

                if (isBooked)
                {
                    TempData["Error"] = "This time slot is already booked. Please choose another time.";
                    return RedirectToAction("Appointments");
                }

                var appointment = new AppointmentDTO
                {
                    PatientUserId = currentUser.Value,
                    DoctorUserId = model.DoctorUserId,
                    AppointmentDateTime = appointmentDateTime,
                    Notes = model.Notes
                };

                var response = await _httpClient.PostAsJsonAsync("Appointments", appointment);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Appointment booked successfully!";
                    return RedirectToAction("Appointments");
                }
                TempData["Error"] = "An error occurred while booking the appointment:";
                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                // Log the inner exception for debugging
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = "An error occurred while booking the appointment: " + innerMessage;
                return RedirectToAction("Appointments");
            }
        }


        [HttpGet]
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            if (currentUser.Value == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var appointment = await GetById(id);
            if (appointment == null || appointment.PatientUserId != currentUser.Value)
            {
                TempData["Error"] = "Appointment not found or access denied.";
                return RedirectToAction("Appointments");
            }

            var appointmentDetail = new AppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDateTime = appointment.AppointmentDateTime,
                Status = appointment.Status ?? "Unknown",
                Notes = appointment.Notes ?? "",
                DoctorName = appointment.DoctorUser?.User?.FullName ?? "Unknown Doctor",
                SpecialtyName = appointment.DoctorUser?.Specialty?.Name ?? "General",
                PatientName = appointment.PatientUser?.User?.FullName ?? "Unknown Patient",
                DoctorAvatarUrl = appointment.DoctorUser?.User?.AvatarUrl ?? "/images/default-doctor.png",
                CreatedAt = appointment.CreatedAt
            };

            return PartialView("_AppointmentDetails", appointmentDetail);
        }
        private async Task<AppointmentDTO> GetById(int id)
        {
            var request = await _httpClient.GetAsync($"Appointments/{id}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (request.IsSuccessStatusCode)
            {
                var appointment = await request.Content.ReadFromJsonAsync<AppointmentDTO>();
                return appointment ?? new AppointmentDTO();
            }
            return new AppointmentDTO();
        }
        [HttpGet]
        public async Task<IActionResult> RescheduleAppointment(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var appointment = await _httpClient.GetFromJsonAsync<AppointmentDTO>($"appointments/{id}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (appointment == null || appointment.PatientUserId != currentUserId.Value)
            {
                TempData["Error"] = "Appointment not found or access denied.";
                return RedirectToAction("Appointments");
            }

            // Check if appointment can be rescheduled (not past date, not completed/cancelled)
            if (appointment.AppointmentDateTime <= DateTime.Now ||
                appointment.Status == "Completed" ||
                appointment.Status == "Cancelled")
            {
                TempData["Error"] = "This appointment cannot be rescheduled.";
                return RedirectToAction("Appointments");
            }

            var model = new BookAppointmentViewModel
            {
                DoctorUserId = appointment.DoctorUserId,
                AppointmentDate = appointment.AppointmentDateTime.Date,
                AppointmentTime = appointment.AppointmentDateTime.TimeOfDay,
                Notes = appointment.Notes
            };

            model.Specialties = (await _httpClient.GetFromJsonAsync<List<SpecialtyDTO>>("Specialties?$expand=Doctors"))
                .Select(s => new SpecialtyViewModel
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();

            ViewBag.AppointmentId = id;
            ViewBag.DoctorName = appointment.DoctorUser?.User?.FullName ?? "Unknown Doctor";
            ViewBag.SpecialtyName = appointment.DoctorUser?.Specialty?.Name ?? "Unknown Specialty";
            ViewBag.SpecialtyId = appointment.DoctorUser?.SpecialtyId ?? 0;

            return PartialView("_RescheduleAppointment", model);
        }

        [HttpPost]
        public async Task<IActionResult> RescheduleAppointment(int id, BookAppointmentViewModel model)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var appointment = await _httpClient.GetFromJsonAsync<AppointmentDTO>($"appointments/{id}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (appointment == null || appointment.PatientUserId != currentUserId.Value)
            {
                TempData["Error"] = "Appointment not found or access denied.";
                return RedirectToAction("Appointments");
            }

            try
            {
                var newAppointmentDateTime = model.AppointmentDate.Add(model.AppointmentTime);

                // Check if new time slot is available (excluding current appointment)
                var isBooked = (await _httpClient.PostAsJsonAsync("Apppointments/GetAvailableTimeSlotsForReschedule", new { model.DoctorUserId, newAppointmentDateTime, id })).IsSuccessStatusCode;

                if (isBooked)
                {
                    TempData["Error"] = "This time slot is already booked. Please choose another time.";
                    return RedirectToAction("Appointments");
                }

                // Update appointment
                appointment.AppointmentDateTime = newAppointmentDateTime;
                appointment.Notes = model.Notes;
                appointment.UpdatedAt = DateTime.Now;

                await _httpClient.PutAsJsonAsync($"Appointments/{appointment.AppointmentId}", appointment);
                TempData["Success"] = "Appointment rescheduled successfully!";
                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while rescheduling: " + ex.Message;
                return RedirectToAction("Appointments");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var appointment = await _httpClient.GetFromJsonAsync<AppointmentDTO>($"appointments/{id}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (appointment == null || appointment.PatientUserId != currentUserId.Value)
            {
                TempData["Error"] = "Appointment not found or access denied.";
                return RedirectToAction("Appointments");
            }

            // Check if appointment can be cancelled
            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
            {
                TempData["Error"] = "This appointment cannot be cancelled.";
                return RedirectToAction("Appointments");
            }

            try
            {
                appointment.Status = "Cancelled";
                appointment.UpdatedAt = DateTime.Now;

                await _httpClient.PutAsJsonAsync($"Appointments/{appointment.AppointmentId}", appointment);
                TempData["Success"] = "Appointment cancelled successfully.";
                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while cancelling: " + ex.Message;
                return RedirectToAction("Appointments");
            }
        }

        [HttpPost("/updateImagePatient")]
        public async Task<IActionResult> UploadImage(IFormFile avatar)
        {
            try
            {
                if (avatar == null || avatar.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var imageUrl = await _photoService.UploadImageAsync(avatar);
                if (imageUrl != null)
                {
                    await _httpClient.PutAsJsonAsync($"User/{currentUser.Value}", new PatientUpdateDTO{ AvatarUrl = imageUrl });

                    return Json(new { success = true, message = "Update image successfully" });
                }

                return Json(new { success = false, message = "Update image error" });
            }
            catch (Exception ex)
            {
                // Trả về một JSON với thông báo lỗi
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
