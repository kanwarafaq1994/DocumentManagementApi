using DocumentManagement.Data.UnitsOfWork;
using LogicApi.ContextHandler;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Common;
using DocumentManagement.Data.Common.Extensions;
using System.Linq;
using LogicApi.Extensions;
using DocumentManagement.Data.Security;
using DocumentManagement.Data.Models;

namespace LogicApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly AttachContext _attachContext;

        public AccountController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, AttachContext attachContext)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _attachContext = attachContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginData)
        {
            try
            {
                var loginResponse = await _unitOfWork._userRepository.CreateUserSession(loginData);

                if (loginResponse.Token == null)
                {
                    return Unauthorized(loginResponse);
                }

                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new InfoDto("Login failed"));
            }
        }
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                try
                {
                    // ActivityMiddleWare is not attached to AccountrController so that a user can login.
                    var token = HttpContext.GetAuthToken();
                    var user = _attachContext.AttachUserToContext(HttpContext, _unitOfWork, token);
                    await _unitOfWork._userRepository.UpdateActivityAsync(user.Id, null);
                }
                catch {  }
                HttpContext.Session.Clear();

                return Ok(new InfoDto("User logged out"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new InfoDto("Logout failed"));
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Registration([FromBody] RegistrationDto registrationDto)
        {
            try
            {
                if (!registrationDto.Email.Equals(registrationDto.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new InfoDto("Email address and confirm Email address must match."));
                }
                
                var userErrors = await _unitOfWork._userRepository.Validate(registrationDto.ToUser());

                if (userErrors.Message.Count > 0)
                {
                    return BadRequest(userErrors.Message);
                }

                var initialUser = registrationDto.ToUser();
                initialUser.PasswordHash = _passwordHasher.HashPassword(initialUser.PasswordHash);
                await _unitOfWork._userRepository.Add(initialUser);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new InfoDto("Registration successful."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new InfoDto("Register failed"));
            }
        }
    }
}
