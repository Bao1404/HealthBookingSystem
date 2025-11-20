using BusinessObject.Models;
using Google.Apis.Gmail.v1.Data;
using HealthBookingSystem.Helper;
using HealthBookingSystem.Models;
using HealthBookingSystem.Service;
using HealthBookingSystemWebApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;
using Services.Interface;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace HealthBookingSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IHubContext<AppointmentHub> _hub;
        private readonly GmailHelper _gmailHelper;
        private readonly PhotoService _photoService;

        private int? currentUser => HttpContext.Session.GetInt32("AccountId");
        private string? currentRole => HttpContext.Session.GetString("Role");
        public DoctorController(GmailHelper gmailHelper, PhotoService photoService, IHttpClientFactory httpClientFactory, IHubContext<AppointmentHub> hub)
        {
            _gmailHelper = gmailHelper;
            _photoService = photoService;
            _httpClient = httpClientFactory.CreateClient("APIClient");
            _hub = hub;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveMenu"] = "Dashboard";
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var dashboardViewModel = await BuildDashboardViewModelAsync(currentUser.Value);
            return View(dashboardViewModel);
        }
        public async Task<IActionResult> Appointments()
        {
            ViewData["ActiveMenu"] = "Appointments";

            //// Get doctor ID from session (you should implement proper authentication)
            //// For demo purposes, assuming doctor ID is stored in session
            //var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1; // Default to 1 for testing
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var doctor = await GetDoctorsById(currentUser.Value);

            var todayAppointments = await GetTodayAppointmentsByDoctor(currentUser.Value);
            var upcomingAppointments = await GetUpcomingAppointment(currentUser.Value);
            var completedAppointments = await GetCompletedAppointmentsByDoctor(currentUser.Value);
            var cancelledAppointments = await GetCancelledAppointmentsByDoctor(currentUser.Value);

            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.UpcomingAppointments = upcomingAppointments;
            ViewBag.CompletedAppointments = completedAppointments;
            ViewBag.CancelledAppointments = cancelledAppointments;

            return View(doctor);
        }
        public async Task<IActionResult> ReloadToday()
        {
            var data = await GetTodayAppointmentsByDoctor(currentUser.Value);
            return PartialView("Partials/DoctorPartials/_TodayAppointment", data);
        }

        public async Task<IActionResult> ReloadUpcoming()
        {
            var data = await GetUpcomingAppointment(currentUser.Value);
            return PartialView("Partials/DoctorPartials/_UpcommingAppointment", data);
        }

        public async Task<IActionResult> ReloadCompleted()
        {
            var data = await GetCompletedAppointmentsByDoctor(currentUser.Value);
            return PartialView("Partials/DoctorPartials/_CompletedAppointment", data);
        }

        public async Task<IActionResult> ReloadCancelled()
        {
            var data = await GetCancelledAppointmentsByDoctor(currentUser.Value);
            return PartialView("Partials/DoctorPartials/_CancelledAppointment", data);
        }
        public async Task<IActionResult> ReloadDashboardToday()
        {
            var data = await GetTodayAppointmentsByDoctor(currentUser.Value);
            return PartialView("Partials/DoctorPartials/_DashboardTodayAppointment", data);
        }
        public async Task<IActionResult> Patients(string filter = "all", string search = "")
        {
            ViewData["ActiveMenu"] = "Patients";
            if(currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var patientsViewModel = await BuildPatientsViewModelAsync(currentUser.Value, filter, search);
            var doctor = await GetDoctorsById(currentUser.Value);
            ViewBag.Doctor = doctor;
            ViewBag.CurrentSearch = search;

            return View(patientsViewModel);
        }

        public async Task<IActionResult> PatientDetails(int id)
        {
            if(currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if(currentRole != "Doctor")
            {
                return RedirectToAction("Index", "User");
            }
            var patient = await GetPatientDetail(id);
            var doctor = await GetDoctorsById(currentUser.Value);
            ViewBag.Doctor = doctor;
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Patients");
            }

            // Check if this patient belongs to the current doctor
            var hasAccess = patient.Appointments.Any(a => a.DoctorUserId == currentUser.Value);
            if (!hasAccess)
            {
                TempData["ErrorMessage"] = "You don't have access to this patient's information.";
                return RedirectToAction("Patients");
            }

            var viewModel = await BuildPatientDetailsViewModelAsync(patient, currentUser.Value);
            return View(viewModel);
        }
        [HttpPost("appointment/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(IFormCollection form)
        {
            var appointmentId = int.Parse(form["appointmentId"]);
            var status = form["status"];
            var appointment = new AppointmentDTO
            {
                Status = status
            };
            var response = await _httpClient.PutAsJsonAsync($"appointments/{appointmentId}", appointment);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to update appointment status.");
            }
            await _hub.Clients.All.SendAsync("ReceiveAppointmentUpdate", appointmentId, status);
            var upcomingAppointments = await GetUpcomingAppointment(currentUser.Value);
            return PartialView("Partials/DoctorPartials/_UpcommingAppointment", upcomingAppointments);
        }
        private async Task<PatientDTO> GetPatientDetail(int id)
        {
            var request = await _httpClient.GetAsync($"patients/{id}?$expand=User,Appointments($expand=DoctorUser($expand=User),MedicalRecords),MedicalHistories");
            if(request.IsSuccessStatusCode)
            {
                var patient = await request.Content.ReadFromJsonAsync<PatientDTO>();
                return patient ?? new PatientDTO();
            }
            return new PatientDTO();
        }
        private async Task<List<PatientDTO>> SearchPatients(int doctorId, string search)
        {
            var request = await _httpClient.GetAsync($"patients/doctor/{doctorId}/search?query={search}$expand=User,Appointments($expand=DoctorUser)");
            if(request.IsSuccessStatusCode)
            {
                var patients = await request.Content.ReadFromJsonAsync<List<PatientDTO>>();
                return patients ?? new List<PatientDTO>();
            }
            return new List<PatientDTO>();
        }
        private async Task<PatientsViewModel> BuildPatientsViewModelAsync(int doctorId, string filter, string search)
        {
            List<PatientDTO> patients = await GetPatientsByDoctor(doctorId);

            // Apply search filter first if provided
            if (!string.IsNullOrEmpty(search))
            {
                patients = await SearchPatients(doctorId, search);
            }

            var patientInfos = patients.Select(p => MapToPatientInfo(p)).ToList();

            // Get counts for filters (always get all data for counts)
            var allPatients = await GetPatientsByDoctor(doctorId);

            return new PatientsViewModel
            {
                AllPatients = allPatients.Select(p => MapToPatientInfo(p)).ToList(),
                Patients = patientInfos,
                CurrentFilter = filter,
                TotalPatients = allPatients.Count(),
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

            // Determine patient status
            var status = GetPatientStatus(patient);

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
                CreatedAtDisplay = patient.CreatedAt?.ToString("MMM dd, yyyy") ?? "N/A",
                RecentAppointments = doctorAppointments.TakeLast(3).Select(a => new RecentAppointment
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = a.Status ?? "Unknown",
                    Notes = a.Notes ?? "",
                    AppointmentType = GetAppointmentType(a.Notes),
                    DateDisplay = a.AppointmentDateTime.ToString("MMM dd"),
                    TimeDisplay = a.AppointmentDateTime.ToString("HH:mm")
                }).ToList()
            };
        }

        private async Task<PatientDetailsViewModel> BuildPatientDetailsViewModelAsync(PatientDTO patient, int doctorId)
        {
            var doctorAppointments = patient.Appointments
                .Where(a => a.DoctorUserId == doctorId)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToList();

            var appointmentHistory = doctorAppointments.Select(a => new AppointmentHistory
            {
                AppointmentId = a.AppointmentId,
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status ?? "Unknown",
                Notes = a.Notes ?? "",
                AppointmentType = GetAppointmentType(a.Notes),
                DoctorNotes = a.Notes ?? "",
                CreatedAt = a.CreatedAt
            }).ToList();

            var statistics = new PatientStatistics
            {
                TotalAppointments = doctorAppointments.Count,
                CompletedAppointments = doctorAppointments.Count(a => a.Status == "Completed"),
                CancelledAppointments = doctorAppointments.Count(a => a.Status == "Cancelled"),
                FirstAppointment = doctorAppointments.LastOrDefault()?.AppointmentDateTime,
                LastAppointment = doctorAppointments.FirstOrDefault()?.AppointmentDateTime,
                PatientSince = patient.CreatedAt?.ToString("MMMM yyyy") ?? "Unknown"
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

        private string GetPatientStatus(PatientDTO patient)
        {
            var recentAppointments = patient.Appointments
                .Where(a => a.AppointmentDateTime >= DateTime.Now.AddDays(-30))
                .ToList();

            if (recentAppointments.Any(a => a.Notes != null && a.Notes.ToLower().Contains("emergency")))
                return "Critical";

            if (recentAppointments.Any(a => a.Notes != null && a.Notes.ToLower().Contains("follow-up")))
                return "Follow-up";

            if (patient.CreatedAt >= DateTime.Now.AddDays(-30))
                return "New";

            if (recentAppointments.Any(a => a.Status == "Completed" || a.Status == "Confirmed"))
                return "Active";

            return "Inactive";
        }

        public async Task<IActionResult> Schedule(DateTime? week)
        {
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Doctor = await GetDoctorsById(currentUser.Value);
            ViewData["ActiveMenu"] = "Schedule";

            var currentWeek = week ?? DateTime.Now;

            var scheduleViewModel = await BuildScheduleViewModelAsync(currentUser.Value, currentWeek);

            return View(scheduleViewModel);
        }
        public async Task<IActionResult> ProfileAsync()
        {
            ViewData["ActiveMenu"] = "Profile";
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var doctor = await GetDoctorsById(currentUser.Value);
            ViewBag.Specialties = await GetAllSpecialties();
            return View(doctor);
        }
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            ViewData["ActiveMenu"] = "Calendar";

            var currentDate = year.HasValue && month.HasValue
                ? new DateTime(year.Value, month.Value, 1)
                : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var calendarViewModel = await BuildCalendarViewModelAsync(currentUser.Value, currentDate);
            ViewBag.Doctor = await GetDoctorsById(currentUser.Value);

            return View(calendarViewModel);
        }
        public async Task<IActionResult> Messages()
        {
            ViewData["ActiveMenu"] = "Messages";
            if(currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewData["DoctorId"] = currentUser.Value;
            var doctor = await GetDoctorsById(currentUser.Value);
            return View(doctor);
        }
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            var appointment = await GetAppointmentById(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
            if (appointment.DoctorUserId != doctorId)
            {
                return Forbid();
            }

            return View(appointment);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(IFormCollection form)
        {
            var doctorId = currentUser.Value;
            var name = form["name"];
            var phone = form["phone"];
            var specialtyId = int.Parse(form["specialty"]);
            var bio = form["bio"];
            var exp = form["exp"];
            var updateDto = new DoctorUpdateDTO
            {
                FullName = name,
                PhoneNumber = phone,
                Bio = bio,
                Experience = exp,
                SpecialtyId = specialtyId
            };

            var response = await _httpClient.PutAsJsonAsync($"doctors/{doctorId}", updateDto);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Failed to update profile.";
                TempData["Type"] = "error";
                return RedirectToAction("Profile");
            }
            TempData["Message"] = "Profile updated successfully.";
            TempData["Type"] = "success";
            return RedirectToAction("Profile");
        }
        [HttpGet]
        public async Task<IActionResult> GetAppointmentInfo(int appointmentId)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var appointment = await GetAppointmentById(appointmentId);

                if (appointment == null || appointment.DoctorUserId != doctorId)
                {
                    return Json(new { success = false, message = "Appointment not found or unauthorized." });
                }

                return Json(new
                {
                    success = true,
                    patientName = appointment.PatientUser.User.FullName,
                    appointmentDate = appointment.AppointmentDateTime.ToString("MMM dd, yyyy - HH:mm"),
                    notes = appointment.Notes ?? "No additional notes"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving appointment information." });
            }
        }
        private async Task<AppointmentDTO> GetAppointmentById(int id)
        {
            var request = await _httpClient.GetAsync($"appointments/{id}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (request.IsSuccessStatusCode)
            {
                var appointment = await request.Content.ReadFromJsonAsync<AppointmentDTO>();
                return appointment ?? new AppointmentDTO();
            }
            return new AppointmentDTO();
        }
        private async Task<List<AppointmentDTO>> GetAppointmentByMonth(int doctorId)
        {
            var request = await _httpClient.GetAsync($"appointments/month/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if(request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        private async Task<CalendarViewModel> BuildCalendarViewModelAsync(int doctorId, DateTime currentMonth)
        {
            // Get appointments for the month
            var monthAppointments = await GetAppointmentByMonth(doctorId);

            // Get today's appointments
            var todayAppointments = await GetTodayAppointmentsByDoctor(doctorId);

            // Get upcoming appointments (next 7 days)
            var upcomingAppointments = await GetUpcomingAppointment(doctorId);
            var nextWeekAppointments = upcomingAppointments.Where(a => a.AppointmentDateTime <= DateTime.Now.AddDays(7)).ToList();

            var calendarViewModel = new CalendarViewModel
            {
                CurrentMonth = currentMonth,
                DoctorId = doctorId,
                TodayAppointments = MapToCalendarItems(todayAppointments),
                UpcomingAppointments = MapToCalendarItems(nextWeekAppointments),
                CalendarDays = BuildCalendarDays(currentMonth, monthAppointments)
            };

            return calendarViewModel;
        }

        private List<CalendarDay> BuildCalendarDays(DateTime currentMonth, List<AppointmentDTO> monthAppointments)
        {
            var calendarDays = new List<CalendarDay>();

            // Get the first day of the month and find the start of the calendar grid
            var firstDayOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

            // Build 42 days (6 weeks) for the calendar grid
            for (int i = 0; i < 42; i++)
            {
                var currentDate = startDate.AddDays(i);
                var dayAppointments = monthAppointments
                    .Where(a => a.AppointmentDateTime.Date == currentDate.Date)
                    .ToList();

                calendarDays.Add(new CalendarDay
                {
                    Date = currentDate,
                    IsCurrentMonth = currentDate.Month == currentMonth.Month,
                    IsToday = currentDate.Date == DateTime.Today,
                    Appointments = MapToCalendarItems(dayAppointments),
                    AppointmentCount = dayAppointments.Count
                });
            }

            return calendarDays;
        }

        private List<AppointmentCalendarItem> MapToCalendarItems(List<AppointmentDTO> appointments)
        {
            return appointments.Select(a => new AppointmentCalendarItem
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.PatientUser?.User?.FullName ?? "Unknown Patient",
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status ?? "Unknown",
                Notes = a.Notes ?? "",
                AppointmentType = GetAppointmentType(a.Notes),
                StatusColor = GetStatusColor(a.Status),
                TimeDisplay = a.AppointmentDateTime.ToString("HH:mm"),
                DateDisplay = a.AppointmentDateTime.ToString("MMM dd")
            }).ToList();
        }

        private string GetAppointmentType(string? notes)
        {
            if (string.IsNullOrEmpty(notes)) return "General";

            var lowerNotes = notes.ToLower();
            if (lowerNotes.Contains("follow-up")) return "Follow-up";
            if (lowerNotes.Contains("check-up")) return "Check-up";
            if (lowerNotes.Contains("emergency")) return "Emergency";
            if (lowerNotes.Contains("consultation")) return "Consultation";

            return "General";
        }

        private string GetStatusColor(string? status)
        {
            return status?.ToLower() switch
            {
                "upcoming" => "primary",
                "completed" => "success",
                "cancelled" => "danger",
                _ => "secondary"
            };
        }
        private async Task UpdateDoctorAvatar(string imageUrl, int doctorId)
        {

        }
        [HttpPost("/sendMail")]
        public async Task<IActionResult> SendEmailAsync()
        {
            try
            {
                // Gọi phương thức SendEmailAsync từ GmailHelper để gửi email
                await _gmailHelper.SendEmailAsync();

                // Trả về một JSON với thông báo thành công
                return Json(new { success = true, message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                // Trả về một JSON với thông báo lỗi
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost("/updateImageDoctor")]
        public async Task<IActionResult> UploadImage(IFormFile avatar)
        {
            try
            {
                if (avatar == null || avatar.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var imageUrl = await _photoService.UploadImageAsync(avatar);
                if(imageUrl != null)
                {
                   // await _doctorService.UpdateImageUrlDoctor(imageUrl, currentUser.Value);

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
        [HttpGet]
        public async Task<IActionResult> GetWeeklySchedule(DateTime week)
        {
            try
            {              
                var scheduleViewModel = await BuildScheduleViewModelAsync(currentUser.Value, week);

                return Json(new
                {
                    success = true,
                    weeklySchedule = scheduleViewModel.WeeklySchedule,
                    currentWeekDisplay = scheduleViewModel.CurrentWeekDisplay
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Error loading weekly schedule." });
            }
        }
        private async Task<ScheduleViewModel> BuildScheduleViewModelAsync(int doctorId, DateTime currentWeek)
        {
            // Get start and end of week
            var startOfWeek = currentWeek.AddDays(-(int)currentWeek.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            // Get appointments for the week
            var weekAppointments = await GetAppointmentsByWeekAsync(doctorId);

            // Build weekly schedule
            var weeklySchedule = new List<WeeklyScheduleDay>();
            for (int i = 0; i < 7; i++)
            {
                var currentDate = startOfWeek.AddDays(i);
                var dayAppointments = weekAppointments
                    .Where(a => a.AppointmentDateTime.Date == currentDate.Date)
                    .ToList();

                var daySchedule = new WeeklyScheduleDay
                {
                    DayName = currentDate.ToString("dddd"),
                    Date = currentDate,
                    IsToday = currentDate.Date == DateTime.Today,
                    Appointments = MapToScheduleAppointments(dayAppointments),
                    AvailableSlots = GenerateAvailableSlots(currentDate, dayAppointments)
                };

                weeklySchedule.Add(daySchedule);
            }

            return new ScheduleViewModel
            {
                DoctorId = doctorId,
                CurrentWeek = currentWeek,
                CurrentWeekDisplay = $"{startOfWeek:MMM dd} - {endOfWeek:MMM dd}, {currentWeek:yyyy}",
                WeeklySchedule = weeklySchedule
            };
        }

        private List<ScheduleAppointment> MapToScheduleAppointments(List<AppointmentDTO> appointments)
        {
            return appointments.Select(a => new ScheduleAppointment
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.PatientUser?.User?.FullName ?? "Unknown Patient",
                PatientEmail = a.PatientUser?.User?.Email ?? "",
                PatientPhone = a.PatientUser?.User?.PhoneNumber ?? "",
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status ?? "Unknown",
                Notes = a.Notes ?? "",
                AppointmentType = GetAppointmentType(a.Notes),
                StatusColor = GetStatusColor(a.Status),
                TimeDisplay = a.AppointmentDateTime.ToString("HH:mm"),
                Duration = "30 min"
            }).ToList();
        }

        private List<TimeSlot> GenerateAvailableSlots(DateTime date, List<AppointmentDTO> appointments)
        {
            var slots = new List<TimeSlot>();
            var startTime = new TimeSpan(9, 0, 0); // 9:00 AM
            var endTime = new TimeSpan(17, 00, 0); // 5:00 PM
            var slotDuration = new TimeSpan(0, 30, 0); // 30 minutes

            for (var time = startTime; time < endTime; time += slotDuration)
            {
                var slotEndTime = time + slotDuration;
                var isAvailable = !appointments.Any(a =>
                    a.AppointmentDateTime.TimeOfDay >= time &&
                    a.AppointmentDateTime.TimeOfDay < slotEndTime);

                slots.Add(new TimeSlot
                {
                    TimeSlotId = slots.Count + 1,
                    Date = date,
                    StartTime = time,
                    EndTime = slotEndTime,
                    SlotType = "regular",
                    Notes = "",
                    IsAvailable = isAvailable,
                    TimeDisplay = $"{time:hh\\:mm} - {slotEndTime:hh\\:mm}",
                    DayName = date.ToString("dddd")
                });
            }

            return slots;
        }
        private async Task<List<SpecialtyDTO>> GetAllSpecialties()
        {
            var request = await _httpClient.GetAsync("Specialties?$expand=Doctors");
            if(request.IsSuccessStatusCode)
            {
                var specialties = await request.Content.ReadFromJsonAsync<List<SpecialtyDTO>>();
                return specialties ?? new List<SpecialtyDTO>();
            }
            return new List<SpecialtyDTO>();

        }
        private async Task<DoctorDTO> GetDoctorsById(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Doctors/{doctorId}?$expand=Appointments,User,Specialty,TimeOffs,WorkingHours");
            if(request.IsSuccessStatusCode)
            {
                var doctor = await request.Content.ReadFromJsonAsync<DoctorDTO>();
                return doctor ?? new DoctorDTO();
            }
            return new DoctorDTO();
        }
        private async Task<List<AppointmentDTO>> GetUpcomingAppointment(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Appointments/doctor/{doctorId}?&$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)&$filter=Status eq 'Upcoming'&$orderby=AppointmentDateTime asc");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        private async Task<List<AppointmentDTO>> GetTodayAppointmentsByDoctor(int doctorId)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var filter = $"date(AppointmentDateTime) eq {today}&$orderby=AppointmentDateTime asc";
            var request = await _httpClient.GetAsync($"Appointments/doctor/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)&filter={filter}");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        private async Task<List<AppointmentDTO>> GetCompletedAppointmentsByDoctor(int doctorId)
        {
            var filter = $"Status eq 'Completed'&$orderby=AppointmentDateTime asc";
            var request = await _httpClient.GetAsync($"Appointments/doctor/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)&filter={filter}");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        private async Task<List<AppointmentDTO>> GetCancelledAppointmentsByDoctor(int doctorId)
        {
            var filter = $"Status eq 'Cancelled'&$orderby=AppointmentDateTime asc";
            var request = await _httpClient.GetAsync($"Appointments/doctor/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)&filter={filter}");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        private async Task<List<PatientDTO>> GetPatientsByDoctor(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Patients/doctor/{doctorId}?$expand=Appointments,MedicalHistories,User");
            if (request.IsSuccessStatusCode)
            {
                var patients = await request.Content.ReadFromJsonAsync<List<PatientDTO>>();
                return patients ?? new List<PatientDTO>();
            }
            return new List<PatientDTO>();
        }
        private async Task<List<AppointmentDTO>> GetAppointmentsByWeekAsync(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Appointments/week/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }
        private async Task<DoctorDashboardViewModel> BuildDashboardViewModelAsync(int doctorId)
        {
            // Get current doctor information
            var currentDoctor = await GetDoctorsById(doctorId);

            // Get appointments data
            var todayAppointments = await GetTodayAppointmentsByDoctor(doctorId);
            //var pendingAppointments = await _appointmentService.GetPendingAppointmentsByDoctorAsync(doctorId);
            var completedAppointments = await GetCompletedAppointmentsByDoctor(doctorId);

            // Get patients data
            var allPatients = await GetPatientsByDoctor(doctorId);
            //var followUpPatients = await _patientService.GetFollowUpPatientsAsync(doctorId);

            // Get weekly stats
            var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            var weeklyAppointments = await GetAppointmentsByWeekAsync(doctorId);

            // Build dashboard stats
            var stats = new DoctorDashboardStats
            {
                TodayAppointmentCount = todayAppointments.Count,
                //TotalPatientCount = allPatients.Count,
                UnreadMessageCount = 12, // TODO: Implement message service
                //FollowUpCount = followUpPatients.Count
            };

            // Build weekly stats
            var weeklyStats = new WeeklyStats
            {
                WeeklyAppointmentCount = weeklyAppointments.Count,
                //WeeklyNewPatientCount = newPatients.Count(p => p.CreatedAt >= weekStart),
                WeeklyFollowUpCount = weeklyAppointments.Count(a => a.Notes?.ToLower().Contains("follow") == true)
            };

            // Get recent patients (last 5 who had appointments)
            var recentPatients = allPatients
                .Where(p => p.Appointments.Any())
                .OrderByDescending(p => p.Appointments.Max(a => a.AppointmentDateTime))
                .Take(5)
                .Select(p => MapToPatientInfo(p))
                .ToList();

            //// Build urgent notifications (mock data for now)
            //var urgentNotifications = BuildUrgentNotifications(pendingAppointments, followUpPatients);


            return new DoctorDashboardViewModel
            {
                CurrentDoctor = currentDoctor ?? new DoctorDTO(),
                Stats = stats,
                TodaySchedule = todayAppointments,
                WeekSchedule = weeklyAppointments,
                WeeklyStats = weeklyStats
            };
        }

        private List<NotificationItem> BuildUrgentNotifications(List<AppointmentDTO> pendingAppointments, List<PatientDTO> followUpPatients)
        {
            var notifications = new List<NotificationItem>();

            // Add pending appointment notifications
            foreach (var appointment in pendingAppointments.Take(3))
            {
                notifications.Add(new NotificationItem
                {
                    Id = appointment.AppointmentId,
                    Type = "appointment",
                    Title = "New Appointment Request",
                    Message = $"{appointment.PatientUser?.User?.FullName} requests appointment on {appointment.AppointmentDateTime:MMM dd, HH:mm}",
                    CreatedAt = appointment.CreatedAt ?? DateTime.Now,
                    IsRead = false,
                    Icon = "fas fa-calendar-plus",
                    Color = "primary"
                });
            }

            // Add follow-up notifications
            foreach (var patient in followUpPatients.Take(2))
            {
                var lastAppointment = patient.Appointments.OrderByDescending(a => a.AppointmentDateTime).FirstOrDefault();
                if (lastAppointment != null)
                {
                    notifications.Add(new NotificationItem
                    {
                        Id = patient.UserId,
                        Type = "follow-up",
                        Title = "Follow-up Required",
                        Message = $"{patient.User.FullName} needs follow-up appointment",
                        CreatedAt = lastAppointment.AppointmentDateTime.AddDays(7),
                        IsRead = false,
                        Icon = "fas fa-user-clock",
                        Color = "warning"
                    });
                }
            }

            return notifications.OrderByDescending(n => n.CreatedAt).ToList();
        }

    }
}