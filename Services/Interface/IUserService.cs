using BusinessObject.Models;
namespace Services.Interface
{
    public interface IUserService
    {
        Task<User> Login(string email, string password);
        Task<User> GetUserById(int userId);
        Task<User> CheckUserExist(string email);
        Task CreateUser(User user);
        
        Task<List<User>> GetAllUsers();
        Task<List<User>> GetUsersByRole(string role);
        Task<List<User>> GetUsersByStatus(bool isActive);
        Task UpdateUser(User user);
        Task DeleteUser(int userId);
        Task<bool> UpdateUserStatus(int userId, bool isActive);
        Task UpdateLastLogin(int userId);
        Task<List<User>> SearchUsers(string searchTerm);
        Task<int> GetTotalUsersCount();
        Task<List<User>> GetUsersWithPagination(int page, int pageSize);
        Task<bool> BulkUpdateUserStatus(List<int> userIds, bool isActive);
        Task<bool> BulkDeleteUsers(List<int> userIds);
    }
}
