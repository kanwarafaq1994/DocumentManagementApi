using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> DoesEmailExist(string email, int userId);
        Task<LoginResponseDto> CreateUserSession(LoginDto loginDto);
        Task<bool> CheckFailedLoginCount(int userId);
        Task<DateTime?> UpdateActivityAsync(int userId, DateTime? lastActivity);
        Task<User> GetUserByEmail(string email);

    }
}
