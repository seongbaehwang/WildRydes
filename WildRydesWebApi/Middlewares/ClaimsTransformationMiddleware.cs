using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace WildRydesWebApi.Middlewares
{
    public static class ClaimsTransformationMiddlewareExtensions
    {
        public static IApplicationBuilder UseClaimsTransformation(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsTransformationMiddleware>();
        }
    }

    public class ClaimsTransformationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IClaimsTransformation _claimsTransformer;

        public ClaimsTransformationMiddleware(RequestDelegate next, IClaimsTransformation claimsTransformer)
        {
            _next = next;
            _claimsTransformer = claimsTransformer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _claimsTransformer.TransformAsync(context.User);

            await _next(context);
        }
    }
}