using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace WildRydesWebApi.Services
{
    public class ClaimsTransformer: IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var ci = (ClaimsIdentity) principal.Identity;

            if (!ci.HasClaim(c => c.Type == ClaimTypes.Name) && ci.HasClaim(c => c.Type == "cognito:username"))
            {
                ci.AddClaim(new Claim(ClaimTypes.Name, ci.Claims.First(c=>c.Type== "cognito:username").Value));
            }

            return Task.FromResult(principal);
        }
    }
}