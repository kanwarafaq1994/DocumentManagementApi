using DocumentManagement.Data.Common;
using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Models;
using DocumentManagement.Data.Repositories;
using DocumentManagement.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Services
{
    public class UserRepository : Repository<User, DocumentManagementContext>, IUserRepository
    {
        private readonly DocumentManagementContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly AppSettings _appSettings;
        private readonly IAuthContext _authContext;
        public UserRepository(DocumentManagementContext context,
            IPasswordHasher passwordHasher,
            IOptions<AppSettings> appSettings,
            IAuthContext authContext) : base(context)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _appSettings = appSettings.Value;
            _authContext = authContext;
        }



        /// <summary>
        /// Get User by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User</returns>
        public override async Task<User> Get(int userId)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
        }

        public override async Task<List<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<bool> DoesEmailExistAsync(string email, int userId)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            bool isExisting = await _context.Users
                .AnyAsync(user => user.Email == email && (userId == 0 || user.Id != userId));

            return isExisting;
        }
        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<LoginResponseDto> CreateUserSession(LoginDto loginDto)
        {
            var currentUser = await GetUserByEmail(loginDto.Email);
            var loginResponse = new LoginResponseDto();

            if (currentUser == null || !currentUser.IsActive)
            {
                loginResponse.Message = "Invalid email address or password";
                return loginResponse;
            }

            if (await CheckFailedLoginCount(currentUser.Id))
            {
                currentUser.IsActive = false;
                await _context.SaveChangesAsync();
                loginResponse.Message = "User has been blocked due to too many failed login attempts";
                return loginResponse;
            }

            if (_passwordHasher.VerifyPassword(currentUser.PasswordHash, loginDto.Password))
            {
                currentUser.FailedLoginCount = 0;
                currentUser.LastActivity = StaticDateTimeProvider.Now;
                await _context.SaveChangesAsync();

                loginResponse.Token = GenerateJwtToken(currentUser);
                loginResponse.Message = "Login successful";
                loginResponse.UserId = currentUser.Id;
            }
            else
            {
                currentUser.FailedLoginCount++;
                await _context.SaveChangesAsync();
                loginResponse.Message = "Invalid email address or password";
            }

            return loginResponse;
        }
        public async Task<bool> CheckFailedLoginCount(int userId)
        {
            return await _context.Users.AnyAsync(f => f.FailedLoginCount > 4 && f.Id == userId);
        }

        public async Task<DateTime?> UpdateActivityAsync(int userId, DateTime? lastActivity)
        {
            var user = await Get(userId);
            user.LastActivity = lastActivity;

            await _context.SaveChangesAsync();
            return user.LastActivity;
        }

        private string GenerateJwtToken(User user)
        {
            // generate token that is valid for 2 hours
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = StaticDateTimeProvider.Now.AddHours(WebConstants.TokenTimeoutHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async override Task<InfoDto> Validate(User entity)
        {
            var errorMessages = new InfoDto();

            if (!entity.Email.IsEmail())
                errorMessages.Message.Add("Invalid email address format");
            else if (await DoesEmailExist(entity.Email, entity.Id))
                errorMessages.Message.Add("The email address is already in use");

            if (string.IsNullOrEmpty(entity.FirstName))
                errorMessages.Message.Add("First name is required");

            if (string.IsNullOrEmpty(entity.LastName))
                errorMessages.Message.Add("Last name is required");

            return errorMessages;
        }
    }
}
