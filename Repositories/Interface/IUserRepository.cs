using BusinessObject.Models;
using System.Collections.Generic;

namespace Repositories.Interface
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Add(User user);
        void Delete(int id);
        Task<User> GetUserByEmailAndPassword(string email, string password);
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
    }
}
