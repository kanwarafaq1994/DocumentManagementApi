using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentManagement.Data.Common.Extensions
{
    public static class AccountHelper
    {
        public static User ToUser(this RegistrationDto registrationDto)
        {
            try
            {
                if (registrationDto == null)
                    return null;

                var createUser = new User()
                {
                    CreatedOn = StaticDateTimeProvider.Now,
                    CreatedBy = 1,
                    IsActive = registrationDto.IsActive,
                    FirstName = registrationDto.FirstName,
                    LastName = registrationDto.LastName,
                    Email = registrationDto.Email,
                    PasswordHash = registrationDto.Password
                };

                return createUser;
            }
            catch (Exception ex)
            {
                throw new Exception("Convert UserDto to User failed", ex);
            }
        }
    }
}
