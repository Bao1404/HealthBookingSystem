using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Repositories.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HealthCareSystemContext _context;
        public UserRepository(HealthCareSystemContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string email, string password)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email) && u.Password.Equals(password));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<User> GetUserById(int userId)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<User> CheckUserExist(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task CreateUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<User> GetAll() => _context.Users.ToList();
        public User GetById(int id) => _context.Users.Find(id);
        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // User management methods using existing properties
        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<User>> GetUsersByRole(string role)
        {
            try
            {
                return await _context.Users.Where(u => u.Role == role).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<User>> GetUsersByStatus(bool isActive)
        {
            try
            {
                return await _context.Users.Where(u => u.IsActive == isActive).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateUser(User user)
        {
 
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
         

        }

        public async Task DeleteUser(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateUserStatus(int userId, bool isActive)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsActive = isActive;
                    user.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateLastLogin(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.UpdatedAt = DateTime.Now; // Using UpdatedAt as LastLogin
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<User>> SearchUsers(string searchTerm)
        {
            try
            {
                return await _context.Users
                    .Where(u => u.FullName.Contains(searchTerm) || 
                               u.Email.Contains(searchTerm))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetTotalUsersCount()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<User>> GetUsersWithPagination(int page, int pageSize)
        {
            try
            {
                return await _context.Users
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
