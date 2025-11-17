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
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace HealthBookingSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IHubContext<AppointmentHub> _hub;
        private readonly IDoctorService _doctorService;
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;
        private readonly GmailHelper _gmailHelper;
        private readonly PhotoService _photoService;
        private readonly ITimeOffService _timeOffService;

        private int? currentUser => HttpContext.Session.GetInt32("AccountId");
        public DoctorController(IDoctorService doctorService, IAppointmentService appointmentService, IUserService userService, IPatientService patientService, GmailHelper gmailHelper, PhotoService photoService, ITimeOffService timeOffService, IHttpClientFactory httpClientFactory, IHubContext<AppointmentHub> hub)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _userService = userService;
            _patientService = patientService;
            _gmailHelper = gmailHelper;
            _photoService = photoService;
            _timeOffService = timeOffService;
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
            var dashboardViewModel = await BuildDashboardViewModelAsync((int)currentUser);
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
            var user = await _userService.GetUserById(currentUser.Value);
            var doctor = await _doctorService.GetDoctorsByIdAsync(currentUser.Value);

            var pendingAppointments = await _appointmentService.GetPendingAppointmentsByDoctorAsync(user.UserId);
            var todayAppointments = await GetTodayAppointmentsByDoctor(user.UserId);
            var upcomingAppointments = await GetUpcomingAppointment(user.UserId);
            var completedAppointments = await GetCompletedAppointmentsByDoctor(user.UserId);
            var cancelledAppointments = await GetCancelledAppointmentsByDoctor(user.UserId);

            ViewBag.PendingAppointments = pendingAppointments;
            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.UpcomingAppointments = upcomingAppointments;
            ViewBag.CompletedAppointments = completedAppointments;
            ViewBag.CancelledAppointments = cancelledAppointments;
            ViewBag.PendingCount = pendingAppointments.Count;

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

        public async Task<IActionResult> ReloadPending()
        {
            var data = await _appointmentService.GetPendingAppointmentsByDoctorAsync(currentUser.Value);
            return PartialView("Partials/DoctorPartials/_PendingAppointment", data);
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
        public async Task<IActionResult> Patients(string filter = "all", string search = "")
        {
            ViewData["ActiveMenu"] = "Patients";

            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
            var patientsViewModel = await BuildPatientsViewModelAsync(doctorId, filter, search);
            var doctor = await _doctorService.GetDoctorsByIdAsync(doctorId);
            ViewBag.Doctor = doctor;
            ViewBag.CurrentSearch = search;

            return View(patientsViewModel);
        }

        public async Task<IActionResult> PatientDetails(int id)
        {
            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
            var patient = await _patientService.GetPatientWithDetailsAsync(id);
            var doctor = await _doctorService.GetDoctorsByIdAsync(doctorId);
            ViewBag.Doctor = doctor;
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Patients");
            }

            // Check if this patient belongs to the current doctor
            var hasAccess = patient.Appointments.Any(a => a.DoctorUserId == doctorId);
            if (!hasAccess)
            {
                TempData["ErrorMessage"] = "You don't have access to this patient's information.";
                return RedirectToAction("Patients");
            }

            //var viewModel = await BuildPatientDetailsViewModelAsync(patient, doctorId);
            //return View(viewModel);
            return View();
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
        private async Task<PatientsViewModel> BuildPatientsViewModelAsync(int doctorId, string filter, string search)
        {
            List<PatientDTO> patients;

            // Apply search filter first if provided
            if (!string.IsNullOrEmpty(search))
            {
                //patients = await _patientService.SearchPatientsAsync(doctorId, search);
            }
            else
            {
                // Get patients based on filter
                patients = filter.ToLower() switch
                {
                    //"active" => await _patientService.GetActivePatientsAsync(doctorId),
                    //"critical" => await _patientService.GetCriticalPatientsAsync(doctorId),
                    //"follow-up" => await _patientService.GetFollowUpPatientsAsync(doctorId),
                    //"new" => _patientService.GetNewPatientsAsync(doctorId, 30),
                    //_ => _patientService.GetPatientsByDoctorAsync(doctorId)
                };
            }

            //var patientInfos = patients.Select(p => MapToPatientInfo(p)).ToList();

            // Get counts for filters (always get all data for counts)
            var allPatients = _patientService.GetPatientsByDoctorAsync(doctorId);
            var activePatients = await _patientService.GetActivePatientsAsync(doctorId);
            var criticalPatients = await _patientService.GetCriticalPatientsAsync(doctorId);
            var followUpPatients = await _patientService.GetFollowUpPatientsAsync(doctorId);
            var newPatients = _patientService.GetNewPatientsAsync(doctorId, 30);

            return new PatientsViewModel
            {
                //AllPatients = allPatients.Select(p => MapToPatientInfo(p)).ToList(),
                //FilteredPatients = patientInfos,
                CurrentFilter = filter,
                //TotalPatients = allPatients.Count,
                ActivePatients = activePatients.Count,
                CriticalPatients = criticalPatients.Count,
                FollowUpPatients = followUpPatients.Count,
                NewPatients = newPatients.Count()
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
            var statusColor = GetPatientStatusColor(status);

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
                PatientStatus = status,
                StatusColor = statusColor,
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

        private string GetPatientStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "active" => "success",
                "critical" => "danger",
                "follow-up" => "warning",
                "new" => "info",
                "inactive" => "secondary",
                _ => "secondary"
            };
        }

        public async Task<IActionResult> Schedule(DateTime? week)
        {
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Doctor = await _doctorService.GetDoctorsByIdAsync(currentUser.Value);
            ViewData["ActiveMenu"] = "Schedule";

            var currentWeek = week ?? DateTime.Now;

            var scheduleViewModel = await BuildScheduleViewModelAsync(currentUser.Value, currentWeek);

            return View(scheduleViewModel);
        }
        public async Task<IActionResult> ProfileAsync()
        {
            if(currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var doctor = await _doctorService.GetDoctorsByIdAsync(currentUser.Value);
            return View(doctor);
        }
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            ViewData["ActiveMenu"] = "Calendar";

            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
            var currentDate = year.HasValue && month.HasValue
                ? new DateTime(year.Value, month.Value, 1)
                : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var calendarViewModel = await BuildCalendarViewModelAsync(doctorId, currentDate);
            ViewBag.Doctor = await _doctorService.GetDoctorsByIdAsync(doctorId);

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
            var doctor = await _doctorService.GetDoctorsByIdAsync(currentUser.Value);
            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAppointment(int appointmentId)
        {
            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
            var result = await _appointmentService.ApproveAppointmentAsync(appointmentId, doctorId);

            if (result)
            {
                TempData["SuccessMessage"] = "Appointment approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve appointment.";
            }

            return RedirectToAction("Appointments");
        }

        [HttpPost]
        public async Task<IActionResult> RejectAppointment(int appointmentId, string? reason)
        {
            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
            var result = await _appointmentService.RejectAppointmentAsync(appointmentId, doctorId, reason);

            if (result)
            {
                TempData["SuccessMessage"] = "Appointment rejected successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject appointment.";
            }

            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> AppointmentDetails(int id)
        {
            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
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
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var appointment = await _appointmentService.GetAppointmentsByIdAsync(appointmentId);

                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Appointments");
                }

                // Check if the appointment belongs to the current doctor
                if (appointment.DoctorUserId != doctorId)
                {
                    TempData["ErrorMessage"] = "You are not authorized to complete this appointment.";
                    return RedirectToAction("Appointments");
                }

                // Check if appointment is in a valid status to be completed
                if (appointment.Status != "Confirmed")
                {
                    TempData["ErrorMessage"] = $"Cannot complete appointment with status '{appointment.Status}'. Only confirmed appointments can be completed.";
                    return RedirectToAction("Appointments");
                }

                // Update appointment status
                appointment.Status = "Completed";
                appointment.UpdatedAt = DateTime.Now;

                await _appointmentService.UpdateAppointmentAsync(appointment);

                TempData["SuccessMessage"] = "Appointment completed successfully!";
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                TempData["ErrorMessage"] = "An error occurred while completing the appointment.";
            }

            return RedirectToAction("Appointments");
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentInfo(int appointmentId)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var appointment = await _appointmentService.GetAppointmentsByIdAsync(appointmentId);

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

        private async Task<CalendarViewModel> BuildCalendarViewModelAsync(int doctorId, DateTime currentMonth)
        {
            // Get appointments for the month
            var monthAppointments = await _appointmentService.GetAppointmentsByMonthAsync(doctorId, currentMonth);

            // Get today's appointments
            var todayAppointments = await _appointmentService.GetTodayAppointmentsByDoctorAsync(doctorId);

            // Get upcoming appointments (next 7 days)
            var upcomingAppointments = await _appointmentService.GetUpcomingAppointmentsByDoctorAsync(doctorId);
            var nextWeekAppointments = upcomingAppointments.Where(a => a.AppointmentDateTime <= DateTime.Now.AddDays(7)).ToList();

            var calendarViewModel = new CalendarViewModel
            {
                CurrentMonth = currentMonth,
                DoctorId = doctorId,
                //TodayAppointments = MapToCalendarItems(todayAppointments),
                //UpcomingAppointments = MapToCalendarItems(nextWeekAppointments),
                //CalendarDays = BuildCalendarDays(currentMonth, monthAppointments)
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
                "pending" => "warning",
                "confirmed" => "primary",
                "completed" => "success",
                "cancelled" => "danger",
                _ => "secondary"
            };
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
                    await _doctorService.UpdateImageUrlDoctor(imageUrl, currentUser.Value);

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

        [HttpPost]
        public async Task<IActionResult> AddTimeOff([FromBody] TimeOffRequest request)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;

                // Parse the request data
                if (!DateOnly.TryParse(request.StartDate, out var startDate) ||
                    !DateOnly.TryParse(request.EndDate, out var endDate))
                {
                    return Json(new { success = false, message = "Invalid date format." });
                }

                // Validate time off
                if (startDate > endDate)
                {
                    return Json(new { success = false, message = "Start date must be before or equal to end date." });
                }

                // Check if time off already exists for this period
                var exists = await _timeOffService.IsTimeOffExistsAsync(doctorId, startDate, endDate);
                if (exists)
                {
                    return Json(new { success = false, message = "Time off already exists for this period." });
                }

                // Create new time off
                var timeOff = new TimeOffDTO
                {
                    DoctorUserId = doctorId,
                    Type = request.Type,
                    Title = request.Title,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsAllDay = request.IsAllDay,
                    Reason = request.Reason
                };

                //var result = await _timeOffService.AddTimeOffAsync(timeOff);

                //if (result)
                //{
                //    return Json(new { success = true, message = "Time off added successfully!" });
                //}
                //else
                //{
                return Json(new { success = false, message = "Failed to add time off." });
                //}
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding time off." });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetWeeklySchedule(DateTime week)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var scheduleViewModel = await BuildScheduleViewModelAsync(doctorId, week);

                return Json(new
                {
                    success = true,
                    weeklySchedule = scheduleViewModel.WeeklySchedule,
                    currentWeekDisplay = scheduleViewModel.CurrentWeekDisplay
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading weekly schedule." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeOff(int id)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var timeOff = await _timeOffService.GetTimeOffByIdAsync(id);

                if (timeOff == null || timeOff.DoctorUserId != doctorId)
                {
                    return Json(new { success = false, message = "Time off not found or unauthorized." });
                }

                return Json(new
                {
                    success = true,
                    timeOff = new
                    {
                        timeOffId = timeOff.TimeOffId,
                        type = timeOff.Type,
                        title = timeOff.Title,
                        startDate = timeOff.StartDate.ToString("yyyy-MM-dd"),
                        endDate = timeOff.EndDate.ToString("yyyy-MM-dd"),
                        isAllDay = timeOff.IsAllDay,
                        reason = timeOff.Reason
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving time off information." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTimeOff([FromBody] TimeOffRequest request)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;

                // Parse the request data
                if (!DateOnly.TryParse(request.StartDate, out var startDate) ||
                    !DateOnly.TryParse(request.EndDate, out var endDate))
                {
                    return Json(new { success = false, message = "Invalid date format." });
                }

                // Validate time off
                if (startDate > endDate)
                {
                    return Json(new { success = false, message = "Start date must be before or equal to end date." });
                }

                // Get existing time off
                var existingTimeOff = await _timeOffService.GetTimeOffByIdAsync(request.TimeOffId);
                if (existingTimeOff == null || existingTimeOff.DoctorUserId != doctorId)
                {
                    return Json(new { success = false, message = "Time off not found or unauthorized." });
                }

                // Check if time off already exists for this period (excluding current)
                var exists = await _timeOffService.IsTimeOffExistsAsync(doctorId, startDate, endDate, request.TimeOffId);
                if (exists)
                {
                    return Json(new { success = false, message = "Time off already exists for this period." });
                }

                // Update time off
                existingTimeOff.Type = request.Type;
                existingTimeOff.Title = request.Title;
                existingTimeOff.StartDate = startDate;
                existingTimeOff.EndDate = endDate;
                existingTimeOff.IsAllDay = request.IsAllDay;
                existingTimeOff.Reason = request.Reason;

                var result = await _timeOffService.UpdateTimeOffAsync(existingTimeOff);

                if (result)
                {
                    return Json(new { success = true, message = "Time off updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update time off." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating time off." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTimeOff(int id)
        {
            try
            {
                var doctorId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var timeOff = await _timeOffService.GetTimeOffByIdAsync(id);

                if (timeOff == null || timeOff.DoctorUserId != doctorId)
                {
                    return Json(new { success = false, message = "Time off not found or unauthorized." });
                }

                var result = await _timeOffService.DeleteTimeOffAsync(id);

                if (result)
                {
                    return Json(new { success = true, message = "Time off deleted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete time off." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting time off." });
            }
        }
        private string GetTimeOffIcon(string type)
        {
            return type.ToLower() switch
            {
                "vacation" => "plane",
                "sick" => "thermometer-half",
                "conference" => "graduation-cap",
                "personal" => "user",
                "holiday" => "gift",
                _ => "calendar-times"
            };
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

            // Get time off list from database
            var timeOffs = await _timeOffService.GetTimeOffsByDoctorAsync(doctorId);
            var timeOffList = timeOffs.Select(t => new TimeOffItem
            {
                TimeOffId = t.TimeOffId,
                Type = t.Type,
                Title = t.Title,
                StartDate = t.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = t.EndDate.ToDateTime(TimeOnly.MinValue),
                IsAllDay = t.IsAllDay ?? true,
                Reason = t.Reason ?? "",
                DateRangeDisplay = $"{t.StartDate:MMM dd} - {t.EndDate:MMM dd}, {t.StartDate:yyyy}",
                Icon = GetTimeOffIcon(t.Type)
            }).ToList();

            return new ScheduleViewModel
            {
                DoctorId = doctorId,
                CurrentWeek = currentWeek,
                CurrentWeekDisplay = $"{startOfWeek:MMM dd} - {endOfWeek:MMM dd}, {currentWeek:yyyy}",
                WeeklySchedule = weeklySchedule,
                TimeOffList = timeOffList
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
        private async Task<DoctorDTO> GetDoctorsById(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Doctors/{doctorId}");
            if(request.IsSuccessStatusCode)
            {
                var doctor = await request.Content.ReadFromJsonAsync<DoctorDTO>();
                return doctor;
            }
            return new DoctorDTO();
        }
        private async Task<List<AppointmentDTO>> GetUpcomingAppointment(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Appointments/doctor/{doctorId}?&$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)&$filter=Status eq 'Upcoming'&$orderby=AppointmentDateTime asc");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments;
            }
            return new List<AppointmentDTO>();
        }
        private async Task<List<AppointmentDTO>> GetTodayAppointmentsByDoctor(int doctorId)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var filter = $"date(AppointmentDateTime) eq {today} and (Status eq 'Upcoming' or Status eq 'Completed')&$orderby=AppointmentDateTime asc";
            var request = await _httpClient.GetAsync($"Appointments/doctor/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser($expand=User)&filter={filter}");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments;
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
                return appointments;
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
                return appointments;
            }
            return new List<AppointmentDTO>();
        }
        private async Task<List<PatientDTO>> GetPatientsByDoctor(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Patients/doctor/{doctorId}?$expand=Appointments,MedicalHistories,User");
            if (request.IsSuccessStatusCode)
            {
                var patients = await request.Content.ReadFromJsonAsync<List<PatientDTO>>();
                return patients;
            }
            return new List<PatientDTO>();
        }
        private async Task<List<AppointmentDTO>> GetAppointmentsByWeekAsync(int doctorId)
        {
            var request = await _httpClient.GetAsync($"Appointments/doctor-week/{doctorId}?$expand=DoctorUser,MedicalRecords,PatientUser");
            if (request.IsSuccessStatusCode)
            {
                var appointments = await request.Content.ReadFromJsonAsync<List<AppointmentDTO>>();
                return appointments;
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
            var followUpPatients = await _patientService.GetFollowUpPatientsAsync(doctorId);

            // Get weekly stats
            var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            var weeklyAppointments = await GetAppointmentsByWeekAsync(doctorId);

            // Build dashboard stats
            var stats = new DoctorDashboardStats
            {
                TodayAppointmentCount = todayAppointments.Count,
                //TotalPatientCount = allPatients.Count,
                UnreadMessageCount = 12, // TODO: Implement message service
                FollowUpCount = followUpPatients.Count
            };

            // Build weekly stats
            var weeklyStats = new WeeklyStats
            {
                WeeklyAppointmentCount = weeklyAppointments.Count,
                //WeeklyNewPatientCount = newPatients.Count(p => p.CreatedAt >= weekStart),
                WeeklyFollowUpCount = weeklyAppointments.Count(a => a.Notes?.ToLower().Contains("follow") == true)
            };

            // Map today's schedule
            var todaySchedule = todayAppointments.Select(a => new AppointmentViewModel
            {
                AppointmentId = a.AppointmentId,
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status ?? "Unknown",
                Notes = a.Notes ?? "",
                PatientName = a.PatientUser.User.FullName ?? "Unknown Patient",
                DoctorName = a.DoctorUser?.User?.FullName ?? "Unknown Doctor",
                SpecialtyName = a.DoctorUser?.Specialty?.Name ?? "General",
                DoctorAvatarUrl = a.PatientUser?.User?.AvatarUrl ?? "/images/default-patient.png",
                CreatedAt = a.CreatedAt ?? DateTime.Now
            }).ToList();

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
                TodaySchedule = todaySchedule,
                RecentPatients = recentPatients,
                //UrgentNotifications = urgentNotifications,
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