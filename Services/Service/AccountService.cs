using BusinessObject.Models;
using Repositories.Interface;
using Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Services.Service
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        public AccountService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public IEnumerable<User> GetAllAccounts()
        {
            // Exclude doctors and patients (assuming Role property)
            return _userRepository.GetAll().Where(u => u.Role != "doctor" && u.Role != "patient");
        }
        public User GetAccountById(int id) => _userRepository.GetById(id);
        public void AddAccount(User user) => _userRepository.Add(user);
        public Task UpdateAccount(User user) => _userRepository.UpdateUser(user);
        public void DeleteAccount(int id) => _userRepository.Delete(id);
    }
} 