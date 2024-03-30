using DocumentManagement.Data.Common;
using DocumentManagement.Data.Models;
using DocumentManagement.Data.UnitsOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace LogicApi.ContextHandler
{
    public class AttachContext
    {
        private readonly AppSettings _appSettings;
        private readonly IAuthContext _authContext;

        public AttachContext(IOptions<AppSettings> appSettings, IAuthContext authContext)
        {
            _appSettings = appSettings.Value;
            _authContext = authContext;
        }

        public User AttachUserToContext(HttpContext context, IUnitOfWork unitOfWork, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            // attach user to context on successful jwt validation
            var user = unitOfWork.userRepository.Get(userId).Result;
            context.Items["User"] = user;
            _authContext.Set(user);
            return user;
        }
    }
}
