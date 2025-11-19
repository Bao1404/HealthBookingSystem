using BusinessObject.Models;
using Google.Apis.Gmail.v1.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

        [HttpGet("CheckUserExist")]
        public async Task<IActionResult> CheckUserExist(string email)
        {
            var users = await _userService.CheckUserExist(email);
            return Ok(users);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(User user)
        {
            await _userService.UpdateUser(user);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUser(id);
            return Ok();
        }

        [HttpPost("BulkUpdateUserStatus")]
        public async Task<IActionResult> BulkUpdateUserStatus(List<int> userIds, bool isActive)
        {
            var result = await _userService.BulkUpdateUserStatus(userIds, isActive);
            if (result)
            {
                return Ok(new { message = "User statuses updated successfully" });
            }
            return BadRequest(new { message = "Failed to update user statuses" });
        }

        [HttpPost("BulkDeleteUsers")]
        public async Task<IActionResult> BulkDeleteUsers(List<int> userIds)
        {
            var result = await _userService.BulkDeleteUsers(userIds);
            if (result)
            {
                return Ok(new { message = "Users deleted successfully" });
            }
            return BadRequest(new { message = "Failed to delete users" });
        }

        [HttpPost("UpdateUserStatus")]
        public async Task<IActionResult> UpdateUserStatus(int userId, bool isActive)
        {
            var result = await _userService.UpdateUserStatus(userId, isActive);
            if (result)
            {
                return Ok(new { message = "User status updated successfully" });
            }
            return BadRequest(new { message = "Failed to update user status" });
        }
    }
}
