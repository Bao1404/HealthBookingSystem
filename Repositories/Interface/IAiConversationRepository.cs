using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IAiConversationRepository
    {
        Task CreateConversation(Aiconversation conversation);
        Task<Aiconversation> GetConversationByUserId(int userId);
        Task UpdateConversation(Aiconversation conversation);
    }
}
