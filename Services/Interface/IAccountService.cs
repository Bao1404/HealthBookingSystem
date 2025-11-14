using BusinessObject.Models;
using System.Collections.Generic;

namespace Services.Interface
{
    public interface IAccountService
    {
        IEnumerable<User> GetAllAccounts();
        User GetAccountById(int id);
        void AddAccount(User user);
        Task UpdateAccount(User user);
        void DeleteAccount(int id);
    }
} 