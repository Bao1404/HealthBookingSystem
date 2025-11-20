using BusinessObject.Models;
using HealthBookingSystem.Models;
using HealthBookingSystem.Service;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Services;
using Services.Interface;
using System.Net.Http;
using System.Threading.Tasks;

namespace HealthBookingSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        private int? currentUser => HttpContext.Session.GetInt32("AccountId");
        private readonly IDoctorService _doctorService;
        private readonly ISpecialtyService _specialtyService;
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IMedicalHistoriesService _medicalHistoriesService;
        private readonly IConversationRepository _conversationRepository;

        private readonly PhotoService _photoService;

        public UserController(IUserService userService, IDoctorService doctorService,
            ISpecialtyService specialtyService, IAppointmentService appointmentService,
            IPatientService patientService,
            IMedicalHistoriesService medicalHistoriesService,
            IConversationRepository conversationRepository, PhotoService photoService
            , IHttpClientFactory httpClientFactory)
        {
            _userService = userService;
            _doctorService = doctorService;
            _specialtyService = specialtyService;
            _appointmentService = appointmentService;
            _patientService = patientService;
            _medicalHistoriesService = medicalHistoriesService;
            _conversationRepository = conversationRepository;
            _photoService = photoService;
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }
        public async Task<IActionResult> Index()
        {
            ViewData["ActiveMenu"] = "Dashboard";
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userService.GetUserById(currentUser.Value);
            var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
            // Filter appointments for current user
            var userAppointments = allAppointments.Where(a => a.PatientUserId == currentUser.Value).ToList();

            var dashboardModel = new DashboardViewModel
            {
                //CurrentUser = user,
                //UpcomingAppointments = GetUpcomingAppointments(userAppointments),
                //RecentAppointments = GetRecentAppointments(userAppointments),
                //TodayAppointments = GetTodayAppointments(userAppointments),
                //Stats = CalculateStats(userAppointments)
            };

            return View("Index", dashboardModel);
        }

        private List<AppointmentViewModel> GetUpcomingAppointments(List<AppointmentDTO> appointments)
        {
            return appointments
                .Where(a => a.AppointmentDateTime > DateTime.Now && (a.Status == "Pending" || a.Status == "Confirmed"))
                .OrderBy(a => a.AppointmentDateTime)
                .Take(5)
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
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                }).ToList();
        }

        private List<AppointmentViewModel> GetRecentAppointments(List<AppointmentDTO> appointments)
        {
            return appointments
                .Where(a => a.AppointmentDateTime <= DateTime.Now)
                .OrderByDescending(a => a.AppointmentDateTime)
                .Take(10)
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
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                }).ToList();
        }

        private List<AppointmentViewModel> GetTodayAppointments(List<AppointmentDTO> appointments)
        {
            return appointments
                .Where(a => a.AppointmentDateTime.Date == DateTime.Today && (a.Status == "Pending" || a.Status == "Confirmed"))
                .OrderBy(a => a.AppointmentDateTime)
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
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                }).ToList();
        }

        private DashboardStats CalculateStats(List<AppointmentDTO> appointments)
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
                    (a.Status == "Pending" || a.Status == "Confirmed")),
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
        public async Task<IActionResult> Appointments()
        {
            ViewData["ActiveMenu"] = "Appointments";
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            var currentUser = await _userService.GetUserById(currentUserId.Value);
            ViewBag.CurrentUser = currentUser;
            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
            var userAppointments = allAppointments.Where(a => a.PatientUserId == currentUserId.Value).ToList();

            var model = new BookAppointmentViewModel();
            model.Specialties = (await GetAllSpecialties())
                .Select(s => new SpecialtyViewModel
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();


            // Add appointment data to ViewBag for the appointments list
            //ViewBag.UpcomingAppointments = GetUpcomingAppointments(userAppointments);

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
                    CreatedAt = a.CreatedAt ?? DateTime.Now
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
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Calendar()
        {
            ViewData["ActiveMenu"] = "Calendar";
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            var currentUser = await _userService.GetUserById(currentUserId.Value);
            ViewBag.CurrentUser = currentUser;

            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
            var userAppointments = allAppointments.Where(a => a.PatientUserId == currentUserId.Value).ToList();

            var model = new BookAppointmentViewModel();
            model.Specialties = (await GetAllSpecialties())
                .Select(s => new SpecialtyViewModel
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();

            // Prepare calendar data
            //ViewBag.TodayAppointments = GetTodayAppointments(userAppointments);
            //ViewBag.WeekAppointments = GetWeekAppointments(userAppointments);
            //ViewBag.MonthAppointments = GetMonthAppointments(userAppointments);
            ViewBag.CurrentDate = DateTime.Now;

            return View(model);
        }

        private List<AppointmentViewModel> GetWeekAppointments(List<AppointmentDTO> appointments)
        {
            var startOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            return appointments
                .Where(a => a.AppointmentDateTime >= startOfWeek &&
                           a.AppointmentDateTime < endOfWeek &&
                           (a.Status == "Pending" || a.Status == "Confirmed"))
                .OrderBy(a => a.AppointmentDateTime)
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
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                }).ToList();
        }

        private List<AppointmentViewModel> GetMonthAppointments(List<AppointmentDTO> appointments)
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            return appointments
                .Where(a => a.AppointmentDateTime >= startOfMonth &&
                           a.AppointmentDateTime < endOfMonth &&
                           (a.Status == "Pending" || a.Status == "Confirmed"))
                .OrderBy(a => a.AppointmentDateTime)
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
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                }).ToList();
        }

        public async Task<IActionResult> Doctors()
        {
            ViewData["ActiveMenu"] = "Doctors";

            var currentUserId = HttpContext.Session.GetInt32("UserId");

            var currentUser = await _userService.GetUserById(currentUserId.Value);
            ViewBag.CurrentUser = currentUser;

            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var doctors =  await _doctorService.GetDoctorsAsync();
            var specialties = await GetAllSpecialties();

            var doctorViewModels = doctors.Select(d => new DoctorViewModel
            {
                UserId = d.UserId,
                FullName = d.User.FullName,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty?.Name,
                Qualifications = d.Qualifications,
                Experience = d.Experience,
                Rating = d.Rating,
                AvatarUrl = d.User.AvatarUrl
            }).ToList();

            ViewBag.Doctors = doctorViewModels;
            ViewBag.Specialties = specialties.Select(s => new SpecialtyViewModel
            {
                SpecialtyId = s.SpecialtyId,
                Name = s.Name,
                Description = s.Description
            }).ToList();

            return View();
        }
        public async Task<IActionResult> Messages(int conversationId)
        {

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login"); 
            }

            var conversation = await _conversationRepository.GetConversationsByPatientId(currentUser.Value); 

            if (conversation == null)
            {
                return RedirectToAction("Index", "Home"); 
            }

            var patient = await _patientService.GetByUserIdAsync(currentUser.Value);

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
            var user = await _userService.GetUserById(currentUser.Value);
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
            var patient = await _patientService.GetByUserIdAsync(currentUser.Value);
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
        public async Task<IActionResult> UpdateProfile()
        {
            var userId = Request.Form["userId"];
            var email = Request.Form["email"];
            var fullName = Request.Form["fullName"];
            var phoneNumber = Request.Form["phone"];
            var dateOfBirth = Request.Form["dob"];
            var address = Request.Form["address"];
            var gender = Request.Form["gender"];
            var emergencyPhoneNumber = Request.Form["ePhone"];

            var user = await _userService.GetUserById(int.Parse(userId));

            user.Email = email;
            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.UpdatedAt = DateTime.Now;

            await _userService.UpdateUser(user);

            var patient = await _patientService.GetByUserIdAsync(int.Parse(userId));

            patient.Address = address;
            patient.DateOfBirth = DateOnly.Parse(dateOfBirth);
            patient.Gender = gender;
            patient.EmergencyPhoneNumber = emergencyPhoneNumber;
            patient.UpdatedAt = DateTime.Now;

            await _patientService.UpdatePatient(patient);

            return RedirectToAction("Profile", "User");

        }
        [HttpPost("Health")]
        public async Task<IActionResult> UpdateHealthProfile()
        {
            var useId = Request.Form["userId"];
            var height = Request.Form["height"];
            var weight = Request.Form["weight"];
            var bloodType = Request.Form["blood"];
            var allergies = Request.Form["allergy"];
            var heightM = double.Parse(height) / 100.0;
            double bmi = double.Parse(weight) / (heightM * heightM);

            var patient = await _patientService.GetByUserIdAsync(int.Parse(useId));
            patient.Height = int.Parse(height);
            patient.Weight = int.Parse(weight);
            patient.BloodType = bloodType;
            patient.Allergies = allergies;
            patient.Bmi = (decimal)bmi;
            patient.UpdatedAt = DateTime.Now;

            await _patientService.UpdatePatient(patient);

            return RedirectToAction("Profile", "User");
        }
        // API Methods for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetDoctorsBySpecialty(int specialtyId)
        {
            // Gọi hàm async nếu có
            var doctors = await _doctorService.GetBySpecialtyAsync(specialtyId);

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
                var isBooked = await _appointmentService.IsTimeSlotBookedAsync(doctorId, appointmentDateTime);

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
                var isBooked = await _appointmentService.IsTimeSlotBookedAsync(doctorId, appointmentDateTime, excludeAppointmentId);

                timeSlots.Add(new TimeSlotViewModel
                {
                    Time = time,
                    IsAvailable = !isBooked && appointmentDateTime > DateTime.Now,
                    DisplayTime = $"{time.Hours:00}:{time.Minutes:00}"
                });
            }

            return PartialView("_TimeSlotOptions", timeSlots);
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                // Reload data for form
                model.Specialties = (await GetAllSpecialties())
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
                var patient = _patientService.GetByUserId(currentUserId.Value);
                if (patient == null)
                {
                    TempData["Error"] = "Patient record not found. Please contact support.";
                    return RedirectToAction("Appointments");
                }

                // Validate that selected doctor exists
                Doctor doctor = null;
                try
                {
                    doctor = _doctorService.GetDoctorById(model.DoctorUserId);
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
                var isBooked = await _appointmentService.IsTimeSlotBookedAsync(model.DoctorUserId, appointmentDateTime);
                if (isBooked)
                {
                    TempData["Error"] = "This time slot is already booked. Please choose another time.";
                    return RedirectToAction("Appointments");
                }

                //var appointment = new AppointmentDTO
                //{
                //    PatientUserId = currentUserId.Value,
                //    DoctorUserId = model.DoctorUserId,
                //    AppointmentDateTime = appointmentDateTime,
                //    Status = "Pending",
                //    Notes = model.Notes,
                //    CreatedAt = DateTime.Now,
                //    UpdatedAt = DateTime.Now
                //};

                //await _appointmentService.AddAppointmentAsync(appointment);
                TempData["Success"] = "Appointment booked successfully!";
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

        // Appointment Management Actions

        [HttpGet]
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
            if (appointment == null || appointment.PatientUserId != currentUserId.Value)
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
                CreatedAt = appointment.CreatedAt ?? DateTime.Now
            };

            return PartialView("_AppointmentDetails", appointmentDetail);
        }

        [HttpGet]
        public async Task<IActionResult> RescheduleAppointment(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
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

            model.Specialties = (await GetAllSpecialties())
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

            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
            if (appointment == null || appointment.PatientUserId != currentUserId.Value)
            {
                TempData["Error"] = "Appointment not found or access denied.";
                return RedirectToAction("Appointments");
            }

            try
            {
                var newAppointmentDateTime = model.AppointmentDate.Add(model.AppointmentTime);

                // Check if new time slot is available (excluding current appointment)
                var isBooked = await _appointmentService.IsTimeSlotBookedAsync(model.DoctorUserId, newAppointmentDateTime, id);
                if (isBooked)
                {
                    TempData["Error"] = "This time slot is already booked. Please choose another time.";
                    return RedirectToAction("Appointments");
                }

                // Update appointment
                appointment.AppointmentDateTime = newAppointmentDateTime;
                appointment.Notes = model.Notes;
                appointment.UpdatedAt = DateTime.Now;

                await _appointmentService.UpdateAppointmentAsync(appointment);
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

            var appointment = await _appointmentService.GetAppointmentsByIdAsync(id);
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

                await _appointmentService.UpdateAppointmentAsync(appointment);
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
                    await _patientService.UpdateImageUrlPatient(imageUrl, currentUser.Value);

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
