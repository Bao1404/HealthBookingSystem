using Azure;
using BusinessObject.Models;
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

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("APIClient");
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
                var statistics = await _client.GetFromJsonAsync<object>("Admin/GetDashboardStatistics");

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
            var doctors = await _client.GetFromJsonAsync<List<User>>("Admin/ManageDoctors");
            return View(doctors);
        }

        public async Task<IActionResult> ManagePatients()
        {
            var patients = await _client.GetFromJsonAsync<List<User>>("Admin/ManagePatients");
            return View(patients);
        }

        // User Detail Pages
        public async Task<IActionResult> UserDetail(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");
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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                    var user = await _client.GetFromJsonAsync<User>($"User/{id}");
                    return View("DoctorEdit", user);
                }

                var existingUser = await _client.GetFromJsonAsync<User>($"User/{id}");
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                if (existingUser.Email != updateUserDto.Email)
                {
                    var emailExists = await _client.GetFromJsonAsync<User>($"User/CheckUserExist?email={updateUserDto.Email}");
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

                await _client.PutAsJsonAsync("User", existingUser);

                TempData["SuccessMessage"] = "Doctor updated successfully!";
                return RedirectToAction("DoctorDetail", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");
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
                    var user = await _client.GetFromJsonAsync<User>($"User/{id}");
                    return View("PatientEdit", user);
                }

                var existingUser = await _client.GetFromJsonAsync<User>($"User/{id}");
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                if (existingUser.Email != updateUserDto.Email)
                {
                    var emailExists = await _client.GetFromJsonAsync<User>($"User/CheckUserExist?email={updateUserDto.Email}");
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

                await _client.PutAsJsonAsync("User", existingUser);

                TempData["SuccessMessage"] = "Patient updated successfully!";
                return RedirectToAction("PatientDetail", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");
                return View("PatientEdit", user);
            }
        }

        public async Task<IActionResult> DoctorEdit(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                    var user = await _client.GetFromJsonAsync<User>($"User/{id}");
                    return View(user);
                }

                var existingUser = await _client.GetFromJsonAsync<User>($"User/{id}");
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                // Check if email is being changed and if it already exists
                if (existingUser.Email != updateUserDto.Email)
                {
                    var emailExists = await _client.GetFromJsonAsync<User>($"User/CheckUserExist?email={updateUserDto.Email}");
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

                await _client.PutAsJsonAsync("User", existingUser);

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("UserDetail", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var response = await _client.GetAsync($"User/{id}");
                var user = await response.Content.ReadFromJsonAsync<User>();
                return View(user);
            }
        }

        public async Task<IActionResult> UserDelete(int id)
        {
            ViewData["ActiveMenu"] = "UserManagement";
            try
            {
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

                if (user == null)
                {
                    return NotFound("User not found");
                }

                await _client.DeleteAsync($"User/{id}");

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
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

                if (user == null)
                {
                    return NotFound("User not found");
                }

                await _client.DeleteAsync($"User/{id}");

                TempData["SuccessMessage"] = "Doctor deleted successfully!";
                return RedirectToAction("ManageDoctors");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");
                return View("DoctorDelete", user);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PatientDeleteConfirmed(int id)
        {
            try
            {
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");

                if (user == null)
                {
                    return NotFound("User not found");
                }

                await _client.DeleteAsync($"User/{id}");

                TempData["SuccessMessage"] = "Patient deleted successfully!";
                return RedirectToAction("ManagePatients");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                var user = await _client.GetFromJsonAsync<User>($"User/{id}");
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
                var allUsers = await _client.GetFromJsonAsync<List<User>>("User");

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

                var responses = new UserListResponseDto
                {
                    //Users = userDtos,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages
                };

                return Json(responses);
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
                // Check if user already exists
                var existingUser = await _client.GetFromJsonAsync<User>($"User/CheckUserExist?email={createUserDto.Email}");
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

                var existingUser = await _client.GetFromJsonAsync<User>($"User/{updateUserDto.UserId}");
                if (existingUser == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Check if email is being changed and if it already exists
                if (existingUser.Email != updateUserDto.Email)
                {

                    var emailExists = await _client.GetFromJsonAsync<User>($"User/CheckUserExist?email={updateUserDto.Email}");
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

                await _client.PutAsJsonAsync("User", existingUser);

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
                var user = await _client.GetFromJsonAsync<User>($"User/{userId}");

                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                await _client.DeleteAsync($"User/{userId}");

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
                        success = (await _client.PostAsJsonAsync( "User/BulkUpdateUserStatus", new { UserIds = bulkActionDto.UserIds, IsActive = true })).IsSuccessStatusCode;
                        break;
                    case "deactivate":
                        success = (await _client.PostAsJsonAsync( "User/BulkUpdateUserStatus", new { UserIds = bulkActionDto.UserIds, IsActive = false })).IsSuccessStatusCode;
                        break;
                    case "delete":
                        success = (await _client.PostAsJsonAsync( "User/BulkDeleteUsers", new { UserIds = bulkActionDto.UserIds })).IsSuccessStatusCode;
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
                    var existingUser = await _client.GetFromJsonAsync<User>($"User/CheckUserExist?email={user.Email}");
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
                var allUsers = await _client.GetFromJsonAsync<List<User>>("User/GetAllUsers");
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
                var success = (await _client.PostAsJsonAsync("User/UpdateUserStatus", new { UserIds = userId , IsActive = isActive })).IsSuccessStatusCode;
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
