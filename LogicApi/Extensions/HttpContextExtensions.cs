using Microsoft.AspNetCore.Http;
using System.Linq;

namespace LogicApi.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetAuthToken(this HttpContext context)
        {
            return context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        }
    }

}
