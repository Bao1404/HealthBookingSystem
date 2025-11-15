using BusinessObject.Models;
using Repositories.Interface;
using Services.Interface;

namespace Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public Task<User> Login(string email, string password) => _userRepository.Login(email, password);
        public Task<User> GetUserById(int userId) => _userRepository.GetUserById(userId);
        public Task<User> CheckUserExist(string email) => _userRepository.CheckUserExist(email);
        public Task CreateUser(User user) => _userRepository.CreateUser(user);

        public Task<List<User>> GetAllUsers() => _userRepository.GetAllUsers();
        public Task<List<User>> GetUsersByRole(string role) => _userRepository.GetUsersByRole(role);
        public Task<List<User>> GetUsersByStatus(bool isActive) => _userRepository.GetUsersByStatus(isActive);
        public Task UpdateUser(User user) => _userRepository.UpdateUser(user);
        public Task DeleteUser(int userId) => _userRepository.DeleteUser(userId);
        public Task<bool> UpdateUserStatus(int userId, bool isActive) => _userRepository.UpdateUserStatus(userId, isActive);
        public Task UpdateLastLogin(int userId) => _userRepository.UpdateLastLogin(userId);
        public Task<List<User>> SearchUsers(string searchTerm) => _userRepository.SearchUsers(searchTerm);
        public Task<int> GetTotalUsersCount() => _userRepository.GetTotalUsersCount();
        public Task<List<User>> GetUsersWithPagination(int page, int pageSize) => _userRepository.GetUsersWithPagination(page, pageSize);

        public async Task<bool> BulkUpdateUserStatus(List<int> userIds, bool isActive)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    await _userRepository.UpdateUserStatus(userId, isActive);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BulkDeleteUsers(List<int> userIds)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    await _userRepository.DeleteUser(userId);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
