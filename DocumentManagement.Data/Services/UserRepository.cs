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
using System.Linq;
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
        public async Task<bool> DoesEmailExist(string email, int userId)
        {
            bool isExisting = false;

            try
            {
                //in case email update scenario
                if (userId > 0)
                    isExisting = await _context.Users.AnyAsync(m =>
                        m.Email == email && m.Id != userId);
                else
                    isExisting = await _context.Users.AnyAsync(m =>
                        m.Email == email);
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }

            return isExisting;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<LoginResponseDto> CreateUserSession(LoginDto loginDto)
        {
            var currentUser = await GetUserByEmail(loginDto.Email);

            var loginResponse = new LoginResponseDto();
            string genericErrorMessage = "E-Mail-Adress and/oder password ist invalid";
            if (currentUser == null)
            {
                loginResponse.Message = genericErrorMessage;
                return loginResponse;
            }

            if (!currentUser.IsActive)
            {
                loginResponse.Message = "User is not active";
                return loginResponse;
            }

            if (await CheckFailedLoginCount(currentUser.Id))
            {
                loginResponse.Message =
                    "You cannot log in because your user has been blocked. Please click on Forgot password.";
                currentUser.IsActive = false;

                await _context.SaveChangesAsync();

                return loginResponse;
            }

            try
            {

                var userDetail = _context.Users
                    .FirstOrDefault(u => u.Email == loginDto.Email);

                if (userDetail != null && _passwordHasher.VerifyPassword(userDetail.PasswordHash, loginDto.Password))
                {
                    loginResponse.Token = GenerateJwtToken(userDetail);
                    loginResponse.Message = "Anmeldung erfolgreich";

                    userDetail.FailedLoginCount = 0;
                    userDetail.LastActivity = StaticDateTimeProvider.Now;

                    await _context.SaveChangesAsync();
                    loginResponse.UserId = userDetail.Id;
                    return loginResponse;
                }

                currentUser.FailedLoginCount += 1;

                loginResponse.Message = genericErrorMessage;

                await _context.SaveChangesAsync();
                return loginResponse;
            }
            catch (Exception ex)
            {
                loginResponse.Message = "Error Occurred: " + ex.Message;
                return loginResponse;
            }
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

        public override Task<InfoDto> Validate(User entity)
        {
            var errorMessages = new InfoDto();
            if (!entity.Email.IsEmail())
            {
                errorMessages.Message.Add("Email address format is not correct");
            }
            else
            {
                var isEmail = entity.Email.IsEmail() || DoesEmailExist(entity.Email, entity.Id).Result;
                if (!isEmail)
                {
                    errorMessages.Message.Add("The email address is already in use");
                }
            }

            if (string.IsNullOrEmpty(entity.FirstName))
            {
                errorMessages.Message.Add("First name could not be empty");
            }

            if (string.IsNullOrEmpty(entity.LastName))
            {
                errorMessages.Message.Add("Last name could not be empty");
            }

            return Task.FromResult(errorMessages);
        }
    }
}
