using DocumentManagement.Data.Common;
using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Helpers;
using DocumentManagement.Data.Models;
using DocumentManagement.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Services
{
    public class UserRepository : Repository<User, DocumentManagementContext>, IUserRepository
    {
        private readonly DocumentManagementContext _context;
        private readonly AppSettings _appSettings;
        private readonly IAuthContext _authContext;
        private readonly int _maxUsers;
        public UserRepository(DocumentManagementContext context,
            IOptions<AppSettings> appSettings,
            IAuthContext authContext) : base(context)
        {
            _context = context;
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
                    .FirstOrDefault(u =>u.Email == loginDto.Email);

                if (userDetail != null && EncryptDecryptHelper.Decrypt(loginDto.Password, userDetail.PasswordHash))
                {
                    loginResponse.Token = GenerateJwtToken(userDetail);
                    loginResponse.Message = "Anmeldung erfolgreich";

                    userDetail.FailedLoginCount = 0;
                    userDetail.LastActivity = StaticDateTimeProvider.Now;

                    await _context.SaveChangesAsync();
                    loginResponse.UserId = userDetail.Id;
                    loginResponse.TwoFactorAuth = userDetail.TwoFactorAuth;
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

        private async Task<bool> MaxUsersReachedAsync(User loginUser)
        {
            if (loginUser.IsLoggedIn())
                return false;

            return await _context.Users.CountAsync(x => x.LastActivity > StaticDateTimeProvider.InactiveTimeLimit) >= _maxUsers;
        }

        public void DeleteUserRoleById(int userId)
        {
            var deletedRole = _context.FpUserRoles.Where(fp => fp.UserId == userId);
            _context.FpUserRoles.RemoveRange(deletedRole);
        }

        public async Task<bool> CheckFailedLoginCount(int userId)
        {
            return await _context.Users.AnyAsync(f => f.FailedLoginCount > 4 && f.Id == userId);
        }

        public async Task<bool> CheckUserHasExactlyOneRole(List<RoleDto> fpUserRoles, Organization organization)
        {
            bool isExist = false;
            if (organization.OrganizationType == OrganizationTypeEnum.Master.ToString())
            {
                isExist = fpUserRoles.Count != 1;
            }

            return await Task.FromResult(isExist);
        }

        public async Task<List<AssignRoleResponseDto>> CheckAllRolesAreAssigned(Organization organization, UserDto userDto)
        {
            var allRoles = await GetAllFpUserRoles(organization.OrganizationType);
            var rolesInOrganization = await GetListofRolesInOrganization(organization.Id, userDto);
            return await UnassignedRoles(rolesInOrganization, allRoles, userDto.Roles);
        }

        public async Task<List<RoleDto>> GetAllFpUserRoles(string organizationType)
        {
            var result = await (from q in _context.Roles.Where(o => o.OrganizationType == organizationType)
                                select new RoleDto { RoleId = q.Id, RoleName = q.Name }).ToListAsync();

            return result;
        }

        public async Task<List<AssignRoleResponseDto>> UnassignedRoles(List<int> rolesInOrganization,
            List<RoleDto> allRoles, List<RoleDto> assignedRoles)
        {
            var listOfAssignedRoles = assignedRoles.Select(a => a.RoleId).ToList();
            var listOfUnAssignRoles = new List<AssignRoleResponseDto>();
            rolesInOrganization = rolesInOrganization.Union(listOfAssignedRoles).ToList();
            foreach (var roles in allRoles)
            {
                var unAssignRoles = rolesInOrganization.Any(roleId => roleId == roles.RoleId);
                if (!unAssignRoles)
                {
                    var role = new AssignRoleResponseDto();

                    role.RoleId = roles.RoleId;
                    role.RoleName = roles.RoleName;
                    role.Message =
                        $"Sie müssen das Benutzerrecht zuerst einem anderen Benutzer in Ihrem Unternehmen zuweisen, da jedes Benutzerrecht mindestens einmal vergeben sein muss {roles.RoleName}.";

                    listOfUnAssignRoles.Add(role);
                }
            }

            return await Task.FromResult(listOfUnAssignRoles);
        }

        public async Task<List<int>> GetListofRolesInOrganization(int organizationId, UserDto userDto)
        {
            List<int> s = new List<int>();
            var organizationUsers = _context.Users.Include(r => r.FpUserRoles)
                .ThenInclude(ur => ur.Roles)
                .Where(x => x.OrganizationId == organizationId && x.Status == StatusEnum.Published.ToString());

            if (!organizationUsers.Any())
            {
                organizationUsers = _context.Users.Include(r => r.FpUserRoles)
                    .ThenInclude(ur => ur.Roles)
                    .Where(x => x.OrganizationId == organizationId && x.Status == StatusEnum.Saved.ToString());
            }

            foreach (var user in organizationUsers)
            {
                if (user.FpUserRoles.Any() && user.Id != userDto.UserId)
                {
                    s.AddRange(user.FpUserRoles.Select(roles => roles.RoleId));
                }
                else if (user.Id == userDto.UserId)
                {
                    s.AddRange(userDto.Roles.Select(roles => roles.RoleId));
                }
            }

            return await Task.FromResult(s.Distinct().ToList());
        }

        public async Task<DateTime?> UpdateActivityAsync(int userId, DateTime? lastActivity)
        {
            var user = await GetById(userId);
            user.LastActivity = lastActivity;

            await _context.SaveChangesAsync();
            return user.LastActivity;
        }

        private string GenerateJwtToken(User user)
        {
            // generate token that is valid for 1 day
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

        public override async Task<InfoDto> Validate(User entity)
        {
            return await Validate(entity, OrganizationTypeEnum.Applicant.ToString());
        }

        public async Task<InfoDto> Validate(User dto, string organizationType)
        {
            var errorMessages = new InfoDto();
            if (dto.OrganizationId is 0 && !dto.Email.IsEmail())
            {
                errorMessages.Message.Add("Das Format der eingetragenen E-Mail-Adresse ist nicht korrekt");
            }
            else
            {
                var isEmail = dto.Email.IsEmail() || DoesEmailExist(dto.Email, dto.Id, _context.Organizations.Single(o => o.Id == dto.OrganizationId).OrganizationKey).Result;
                if (!isEmail)
                {
                    errorMessages.Message.Add("Die angegebene E-Mail-Adresse ist bereits vergeben");
                }
            }

            if (string.IsNullOrEmpty(dto.FirstName))
            {
                errorMessages.Message.Add("Das Feld Vorname darf nicht leer sein");
            }

            if (string.IsNullOrEmpty(dto.LastName))
            {
                errorMessages.Message.Add("Das Feld Nachname darf nicht leer sein");
            }

            if (dto.PhoneNumber != null && !dto.PhoneNumber.IsPhoneNumber())
            {
                errorMessages.Message.Add("Die Telefonnummer ist ungültig");
            }

            var areRolesAllowed = await AreRolesInOrganisationType(dto.FpUserRoles, organizationType);
            if (!areRolesAllowed)
            {
                errorMessages.Message.Add("Ungültige Rolle");
            }

            return errorMessages;
        }

        public async Task<InfoDto> CheckLastUserInOrganization(User toDeletedUser, Organization toDeletedUserOrga)
        {
            //check if user is last user of the organization
            var usersInOrga = await GetAll(toDeletedUser.OrganizationId);
            if (usersInOrga.Count == 1)
            {

                return new InfoDto("Sie dürfen den letzten Benutzer nicht löschen.");
            }

            //check if all user roles are assigned at least once after deletion
            var userDto = toDeletedUser.ToUserDto();
            userDto.Roles.Clear();
            var unassignedRoles = await CheckAllRolesAreAssigned(toDeletedUserOrga, userDto);
            if (unassignedRoles.Any())
            {
                return new InfoDto("Bitte weisen Sie die Benutzerrollen einem anderen Benutzer zu, bevor Sie den Benutzer löschen: " + String.Join(", ", unassignedRoles.Select(x =>
                {
                    try
                    {
                        //use RoleId to convert to Enum then to Localized String
                        return ((RoleEnum)x.RoleId).ToLocalizedString();
                    }
                    catch
                    {
                        //return english RoleName from FE if everything fails
                        return x.RoleName;
                    }
                }))
                + ".");
            }

            return null;
        }

        public async Task<bool> AreRolesInOrganisationType(List<FpUserRoles> userRoles, string organizationType)
        {
            var roles = await _context.Roles.Where(r => r.OrganizationType == organizationType).Select(r => new FpUserRoles() { RoleId = r.Id }).ToListAsync();
            var result = userRoles.Where(f => roles.All(f2 => f2.RoleId != f.RoleId)).ToList();

            return result.Count == 0;
        }

        public Task<PasswordTokenDTO> GenerateResetPasswordLink(User user)
        {
            var passwordTokenDTO = new PasswordTokenDTO();
            passwordTokenDTO.ResetToken = JwtTokenGenerator.GenerateResetPasswordToken(user);

            if (passwordTokenDTO.ResetToken == null) throw new InvalidDataException("invalid token");

            passwordTokenDTO.ResetTokenEncrypted = _cryptographicProvider.Encrypt(passwordTokenDTO.ResetToken);
            passwordTokenDTO.LinkUrl = _appSettings.VMURL + "setpass?token=" + passwordTokenDTO.ResetToken;

            return Task.FromResult(passwordTokenDTO);
        }


        public async Task<InfoDto> ChangeEmailValidation(ChangeEmailDto changeEmailDto, User currentUser)
        {

            if (!changeEmailDto.NewEmail.IsEmail())
            {
                return await Task.FromResult(new InfoDto("Das Format der eingetragenen E-Mail-Adresse ist nicht korrekt."));
            }

            if (changeEmailDto.NewEmail != changeEmailDto.RepeatNewEmail)
            {
                return await Task.FromResult(new InfoDto("Die E-Mail-Adressen müssen übereinstimmen"));
            }

            if (changeEmailDto.NewEmail == currentUser.Email)
            {
                return await Task.FromResult(new InfoDto("Die neue E-Mail-Adresse unterscheidet sich nicht zur bisherigen E-Mail-Adresse."));
            }

            if (!_cryptographicProvider.VerifyPassword(changeEmailDto.Password, currentUser.PasswordHash))
            {
                return await Task.FromResult(new InfoDto("Das eingegebene Passwort ist nicht korrekt"));
            }

            var emailExist = DoesEmailExist(changeEmailDto.NewEmail, currentUser.Id,
                    _context.Organizations.Single(o => o.Id == currentUser.OrganizationId && o.Status == StatusEnum.Published.ToString())
                        .OrganizationKey).Result;

            if (emailExist)
            {
                return await Task.FromResult(new InfoDto("Die E-Mail-Adresse ist bereits bei einem Benutzer Ihrer Organisation hinterlegt."));
            }

            return null;
        }

        public async Task<InfoDto> ChangePasswordValidation(SavePasswordDto changePasswordDto, User currentUser)
        {
            if (!_cryptographicProvider.VerifyPassword(changePasswordDto.OldPassword, currentUser.PasswordHash))
            {
                return await Task.FromResult(new InfoDto("Das eingegebene Passwort ist nicht korrekt."));
            }
            return null;
        }

        public async Task<string> GetOTP(int userId)
        {
            var user = await Get(userId);
            return user.UserOtp.Otp;
        }

        public async Task<string> GenerateOTP(int userId)
        {
            var otp = _otpProvider.GenerateOTP(null);

            var user = await Get(userId);
            if (user.UserOtp == null)
            {
                user.UserOtp = new UserOtp
                {
                    UserId = userId,
                    Otp = otp
                };
            }
            else
            {
                user.UserOtp.Otp = otp;
                user.UserOtp.GenerationTime = DateTime.UtcNow;
            }

            _context.SaveChanges();

            return otp;

        }

        public async Task<TwoFactorAuthResponseDto> VerifyOTP(TwoFactorAuthRequestDto request)
        {
            var user = await Get(request.UserId);
            if (user.UserOtp.Otp != request.Otp)
                return new TwoFactorAuthResponseDto() { OtpMatch = false, Message = "Codes do not match" };
            if (DateTime.UtcNow.Subtract(user.UserOtp.GenerationTime).TotalMinutes >= 30)
                return new TwoFactorAuthResponseDto() { OtpMatch = false, Message = "Code expired" };

            return new TwoFactorAuthResponseDto { OtpMatch = true, Message = "Two Factor Authentication Successful" };
        }
    }
}
