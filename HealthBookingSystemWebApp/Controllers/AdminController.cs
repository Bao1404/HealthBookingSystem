using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using HealthCareSystem.Controllers.dto;
using BusinessObject.Models;
using System.Security.Cryptography;
using System.Text;

namespace HealthCareSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;

        public AdminController(IUserService userService, IAppointmentService appointmentService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
        }

        public IActionResult Index()
        {
            ViewData["ActiveMenu"] = "Dashboard";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                var allUsers = await _userService.GetAllUsers();
                
                // Patient statistics
                var patients = allUsers.Where(u => u.Role == "patient").ToList();
                var totalPatients = patients.Count;
                var activePatients = patients.Count(p => p.IsActive == true);
                var newPatientsThisMonth = patients.Count(p => p.CreatedAt >= DateTime.Now.AddDays(-30));
                var newPatientsThisWeek = patients.Count(p => p.CreatedAt >= DateTime.Now.AddDays(-7));
                
                // Real appointment data from database
                var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
                var totalAppointments = allAppointments.Count;
                var confirmedAppointments = allAppointments.Count(a => a.Status == "Confirmed");
                var pendingAppointments = allAppointments.Count(a => a.Status == "Pending");
                var newAppointmentsThisMonth = allAppointments.Count(a => a.CreatedAt >= DateTime.Now.AddDays(-30));
                var newAppointmentsThisWeek = allAppointments.Count(a => a.CreatedAt >= DateTime.Now.AddDays(-7));

                var appointmentTrends = new List<object>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.Now.AddMonths(-i).Date.AddDays(1 - DateTime.Now.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var count = allAppointments.Count(a =>
                        a.CreatedAt >= monthStart &&
                        a.CreatedAt <= monthEnd);
                    appointmentTrends.Add(new
                    {
                        month = monthStart.ToString("MMM"),
                        count = count
                    });
                }

                // Daily appointment trends (last 7 days)
                var dailyAppointmentTrends = new List<object>();
                for (int i = 6; i >= 0; i--)
                {
                    var day = DateTime.Now.AddDays(-i).Date;
                    var count = allAppointments.Count(a => a.CreatedAt?.Date == day);
                    dailyAppointmentTrends.Add(new
                    {
                        day = day.ToString("ddd"),
                        date = day.ToString("MMM dd"),
                        count = count
                    });
                }

                // Weekly appointment trends (last 12 weeks)
                var weeklyAppointmentTrends = new List<object>();
                for (int i = 11; i >= 0; i--)
                {
                    var weekStart = DateTime.Now.AddDays(-(i * 7)).Date;
                    var weekEnd = weekStart.AddDays(6);
                    var count = allAppointments.Count(a => a.CreatedAt >= weekStart && a.CreatedAt <= weekEnd);
                    weeklyAppointmentTrends.Add(new { 
                        week = $"Week {12-i}", 
                        period = $"{weekStart:MMM dd} - {weekEnd:MMM dd}",
                        count = count 
                    });
                }

                // Patient age distribution (mock data for now)
                var ageDistribution = new List<object>
                {
                    new { age = "18-25", count = 25 },
                    new { age = "26-35", count = 35 },
                    new { age = "36-45", count = 28 },
                    new { age = "46-55", count = 20 },
                    new { age = "56-65", count = 15 },
                    new { age = "65+", count = 12 }
                };

                // Patient status distribution
                var statusDistribution = new List<object>
                {
                    new { status = "Active", count = activePatients, color = "#10b981" },
                    new { status = "Inactive", count = totalPatients - activePatients, color = "#6b7280" }
                };

                // Recent patient registrations
                var recentPatients = patients
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new { 
                        name = p.FullName, 
                        email = p.Email, 
                        registeredAt = p.CreatedAt?.ToString("MMM dd, yyyy"),
                        status = p.IsActive == true ? "Active" : "Inactive"
                    })
                    .ToList();

                // Appointment overview data (using real data from database)
                var appointmentOverviewData = new List<object>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.Now.AddMonths(-i).Date.AddDays(1 - DateTime.Now.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var confirmedCount = allAppointments.Count(a => 
                        a.Status == "Confirmed" && 
                        a.CreatedAt >= monthStart && 
                        a.CreatedAt <= monthEnd);
                    var pendingCount = allAppointments.Count(a => 
                        a.Status == "Pending" && 
                        a.CreatedAt >= monthStart && 
                        a.CreatedAt <= monthEnd);
                    
                    appointmentOverviewData.Add(new
                    {
                        month = monthStart.ToString("MMM"),
                        confirmed = confirmedCount,
                        pending = pendingCount
                    });
                }

                var statistics = new
                {
                    totalPatients = totalPatients,
                    activePatients = activePatients,
                    newPatientsThisMonth = newPatientsThisMonth,
                    newPatientsThisWeek = newPatientsThisWeek,
                    totalAppointments = totalAppointments,
                    confirmedAppointments = confirmedAppointments,
                    pendingAppointments = pendingAppointments,
                    newAppointmentsThisMonth = newAppointmentsThisMonth,
                    newAppointmentsThisWeek = newAppointmentsThisWeek,
                    appointmentTrends = appointmentTrends,
                    dailyAppointmentTrends = dailyAppointmentTrends,
                    weeklyAppointmentTrends = weeklyAppointmentTrends,
                    appointmentOverviewData = appointmentOverviewData,
                    ageDistribution = ageDistribution,
                    statusDistribution = statusDistribution,
                    recentPatients = recentPatients
                };

                return Json(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> Users()
        {
            ViewData["ActiveMenu"] = "UserManagement";
            return View();
        }

        public async Task<IActionResult> ManageDoctors()
        {
            var users = await _userService.GetAllUsers();
            var doctors = users.Where(u => u.Role != null && u.Role.ToLower() == "doctor").ToList();
            return View(doctors);
        }

        public async Task<IActionResult> ManagePatients()
        {
            var users = await _userService.GetAllUsers();
            var patients = users.Where(u => u.Role != null && u.Role.ToLower() == "patient").ToList();
            return View(patients);
        }

        // User Detail Pages
        public async Task<IActionResult> UserDetail(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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
        // User Detail Pages
        public async Task<IActionResult> DoctorDetail(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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
        // User Detail Pages
        public async Task<IActionResult> PatientDetail(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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

        public async Task<IActionResult> UserEdit(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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
        [HttpPost]
        public async Task<IActionResult> DoctorEdit(int id, [FromForm] UpdateUserDto updateUserDto)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await _userService.GetUserById(id);
                    return View("DoctorEdit", user);
                }

                var existingUser = await _userService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                if (existingUser.Email != updateUserDto.Email)
                {
                    var emailExists = await _userService.CheckUserExist(updateUserDto.Email);
                    if (emailExists != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        return View("DoctorEdit", existingUser);
                    }
                }

                existingUser.FullName = updateUserDto.FullName;
                existingUser.Email = updateUserDto.Email;
                existingUser.PhoneNumber = updateUserDto.PhoneNumber;
                existingUser.Role = updateUserDto.Role;
                existingUser.IsActive = updateUserDto.IsActive;
                existingUser.UpdatedAt = DateTime.Now;

                await _userService.UpdateUser(existingUser);

                TempData["SuccessMessage"] = "Doctor updated successfully!";
                return RedirectToAction("DoctorDetail", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var user = await _userService.GetUserById(id);
                return View("DoctorEdit", user);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PatientEdit(int id, [FromForm] UpdateUserDto updateUserDto)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await _userService.GetUserById(id);
                    return View("PatientEdit", user);
                }

                var existingUser = await _userService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                if (existingUser.Email != updateUserDto.Email)
                {
                    var emailExists = await _userService.CheckUserExist(updateUserDto.Email);
                    if (emailExists != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        return View("PatientEdit", existingUser);
                    }
                }

                existingUser.FullName = updateUserDto.FullName;
                existingUser.Email = updateUserDto.Email;
                existingUser.PhoneNumber = updateUserDto.PhoneNumber;
                existingUser.Role = updateUserDto.Role;
                existingUser.IsActive = updateUserDto.IsActive;
                existingUser.UpdatedAt = DateTime.Now;

                await _userService.UpdateUser(existingUser);

                TempData["SuccessMessage"] = "Patient updated successfully!";
                return RedirectToAction("PatientDetail", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var user = await _userService.GetUserById(id);
                return View("PatientEdit", user);
            }
        }

        public async Task<IActionResult> DoctorEdit(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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
        public async Task<IActionResult> PatientEdit(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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

        [HttpPost]
        public async Task<IActionResult> UserEdit(int id, [FromForm] UpdateUserDto updateUserDto)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await _userService.GetUserById(id);
                    return View(user);
                }

                var existingUser = await _userService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                // Check if email is being changed and if it already exists
                if (existingUser.Email != updateUserDto.Email)
                {
                    var emailExists = await _userService.CheckUserExist(updateUserDto.Email);
                    if (emailExists != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        return View(existingUser);
                    }
                }

                existingUser.FullName = updateUserDto.FullName;
                existingUser.Email = updateUserDto.Email;
                existingUser.PhoneNumber = updateUserDto.PhoneNumber;
                existingUser.Role = updateUserDto.Role;
                existingUser.IsActive = updateUserDto.IsActive;
                existingUser.UpdatedAt = DateTime.Now;

                await _userService.UpdateUser(existingUser);

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("UserDetail", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var user = await _userService.GetUserById(id);
                return View(user);
            }
        }

        public async Task<IActionResult> UserDelete(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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

        public async Task<IActionResult> DoctorDelete(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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
        public async Task<IActionResult> PatientDelete(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _userService.GetUserById(id);
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

        [HttpPost]
        public async Task<IActionResult> UserDeleteConfirmed(int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                await _userService.DeleteUser(id);

                TempData["SuccessMessage"] = "User deleted successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("UserDelete", new { id = id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoctorDeleteConfirmed(int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                await _userService.DeleteUser(id);

                TempData["SuccessMessage"] = "Doctor deleted successfully!";
                return RedirectToAction("ManageDoctors");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                var user = await _userService.GetUserById(id);
                return View("DoctorDelete", user);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PatientDeleteConfirmed(int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                await _userService.DeleteUser(id);

                TempData["SuccessMessage"] = "Patient deleted successfully!";
                return RedirectToAction("ManagePatients");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                var user = await _userService.GetUserById(id);
                return View("PatientDelete", user);
            }
        }

        public IActionResult Contents()
        {
            ViewData["ActiveMenu"] = "ContentManagement";
            return View();
        }

        public IActionResult System()
        {
            ViewData["ActiveMenu"] = "SystemManagement";
            return View();
        }

        public IActionResult Reports()  
        {
            ViewData["ActiveMenu"] = "Reports";
            return View();
        }

        // API Actions for User Management
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
        {
            try
            {
                var allUsers = await _userService.GetAllUsers();
                var filteredUsers = allUsers.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    filteredUsers = filteredUsers.Where(u => 
                        u.FullName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                        u.Email.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
                }

                // Apply role filter
                if (!string.IsNullOrEmpty(filter.Role))
                {
                    filteredUsers = filteredUsers.Where(u => u.Role != null && u.Role.Equals(filter.Role, StringComparison.OrdinalIgnoreCase));
                }

                // Apply status filter
                if (filter.IsActive.HasValue)
                {
                    filteredUsers = filteredUsers.Where(u => u.IsActive == filter.IsActive);
                }

                var totalCount = filteredUsers.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

                // Apply pagination
                var paginatedUsers = filteredUsers
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                //var userDtos = paginatedUsers.Select(MapToUserDto).ToList();

                var response = new UserListResponseDto
                {
                    //Users = userDtos,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user already exists
                var existingUser = await _userService.CheckUserExist(createUserDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email already exists" });
                }

                var user = new UserDTO
                {
                    FullName = createUserDto.FullName,
                    Email = createUserDto.Email,
                    PhoneNumber = createUserDto.PhoneNumber,
                    Role = createUserDto.Role,
                    IsActive = createUserDto.IsActive,
                    Password = createUserDto.Password,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                //await _userService.CreateUser(user);

                // TODO: Send welcome email if requested
                if (createUserDto.SendWelcomeEmail)
                {
                    // Implement email sending logic here
                }

                return Ok(new { message = "User created successfully", userId = user.UserId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _userService.GetUserById(updateUserDto.UserId);
                if (existingUser == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Check if email is being changed and if it already exists
                if (existingUser.Email != updateUserDto.Email)
                {
                    var emailExists = await _userService.CheckUserExist(updateUserDto.Email);
                    if (emailExists != null)
                    {
                        return BadRequest(new { error = "Email already exists" });
                    }
                }

                existingUser.FullName = updateUserDto.FullName;
                existingUser.Email = updateUserDto.Email;
                existingUser.PhoneNumber = updateUserDto.PhoneNumber;
                existingUser.Role = updateUserDto.Role;
                existingUser.IsActive = updateUserDto.IsActive;
                existingUser.UpdatedAt = DateTime.Now;

                await _userService.UpdateUser(existingUser);

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var user = await _userService.GetUserById(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                await _userService.DeleteUser(userId);

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkAction([FromBody] BulkActionDto bulkActionDto)
        {
            try
            {
                if (bulkActionDto.UserIds == null || !bulkActionDto.UserIds.Any())
                {
                    return BadRequest(new { error = "No users selected" });
                }

                bool success = false;

                switch (bulkActionDto.Action.ToLower())
                {
                    case "activate":
                        success = await _userService.BulkUpdateUserStatus(bulkActionDto.UserIds, true);
                        break;
                    case "deactivate":
                        success = await _userService.BulkUpdateUserStatus(bulkActionDto.UserIds, false);
                        break;
                    case "delete":
                        success = await _userService.BulkDeleteUsers(bulkActionDto.UserIds);
                        break;
                    default:
                        return BadRequest(new { error = "Invalid action" });
                }

                if (success)
                {
                    return Ok(new { message = $"Bulk {bulkActionDto.Action} completed successfully" });
                }
                else
                {
                    return BadRequest(new { error = $"Failed to perform bulk {bulkActionDto.Action}" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> CreateTestUsers()
        {
            try
            {
                var testUsers = new List<UserDTO>
                {
                    new UserDTO
                    {
                        FullName = "Dr. John Smith",
                        Email = "doctor.john@test.com",
                        PhoneNumber = "1234567890",
                        Role = "doctor",
                        Password = "password123",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new UserDTO
                    {
                        FullName = "Admin User",
                        Email = "admin@test.com",
                        PhoneNumber = "0987654321",
                        Role = "admin",
                        Password = "password123",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new UserDTO
                    {
                        FullName = "Staff Member",
                        Email = "staff@test.com",
                        PhoneNumber = "5555555555",
                        Role = "staff",
                        Password = "password123",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };

                foreach (var user in testUsers)
                {
                    var existingUser = await _userService.CheckUserExist(user.Email);
                    if (existingUser == null)
                    {
                        //await _userService.CreateUser(user);
                    }
                }

                return Json(new { message = "Test users created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var allUsers = await _userService.GetAllUsers();
                var userRoles = allUsers.Select(u => new { u.UserId, u.FullName, u.Email, u.Role, u.IsActive }).ToList();
                var roleCounts = allUsers.GroupBy(u => u.Role).Select(g => new { Role = g.Key, Count = g.Count() }).ToList();
                
                return Json(new { 
                    totalUsers = allUsers.Count,
                    users = userRoles,
                    roles = allUsers.Select(u => u.Role).Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList(),
                    roleCounts = roleCounts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserStatus(int userId, bool isActive)
        {
            try
            {
                var success = await _userService.UpdateUserStatus(userId, isActive);
                if (success)
                {
                    return Ok(new { message = "User status updated successfully" });
                }
                else
                {
                    return NotFound(new { error = "User not found" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private UserDto MapToUserDto(UserDTO user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role ?? "",
                IsActive = (bool)user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                AvatarUrl = user.AvatarUrl
            };
        }
    }
}   
