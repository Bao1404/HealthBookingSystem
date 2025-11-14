using System.ComponentModel.DataAnnotations;

namespace HealthCareSystem.Controllers.dto
{
    public class UserDto
    {
        public int UserId { get; set; }
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? AvatarUrl { get; set; }
        
        public string Status => IsActive ? "Active" : "Inactive";
        public string LastLogin => UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
    }

    public class CreateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public bool SendWelcomeEmail { get; set; } = false;
    }

    public class UpdateUserDto
    {
        public int UserId { get; set; }
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        [Required]
        public bool IsActive { get; set; } = true;
    }

    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? DateFilter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BulkActionDto
    {
        public List<int> UserIds { get; set; } = new List<int>();
        public string Action { get; set; } = string.Empty; // activate, deactivate, delete
    }

    public class UserListResponseDto
    {
        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
