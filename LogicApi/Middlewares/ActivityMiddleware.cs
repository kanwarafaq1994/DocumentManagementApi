using DocumentManagement.Data.Common;
using DocumentManagement.Data.UnitsOfWork;
using LogicApi.ContextHandler;
using LogicApi.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LogicApi.Middlewares
{
    public class ActivityMiddleware
    {

        private readonly RequestDelegate _next;
        public ActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUnitOfWork userService, AttachContext attachContext)
        {
            var token = context.GetAuthToken();

            try
            {
                var user = attachContext.AttachUserToContext(context, userService, token);
                if (!user.IsLoggedIn())
                {
                    throw new UnauthorizedAccessException("Token is logged out");
                }
                await userService.userRepository.UpdateActivityAsync(user.Id, StaticDateTimeProvider.Now);
                await _next.Invoke(context);
                return;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = Text.Plain;
                await context.Response.WriteAsync("Token invalid");
            }
        }
    }

}
