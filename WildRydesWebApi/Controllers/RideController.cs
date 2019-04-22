using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WildRydesWebApi.Entities;
using WildRydesWebApi.Models;

namespace WildRydesWebApi.Controllers
{
    //[Route("api/[controller]")]
    [Route("[controller]")]
    [ApiController]
    public class RideController : ControllerBase
    {
        private readonly ILogger<RideController> _logger;
        private readonly IDynamoDBContext _dbContext;

        public RideController(ILogger<RideController> logger, IDynamoDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<ActionResult<RideResponse>> Post(RideRequest request)
        {
            _logger.LogInformation("{@LambdaContext}", Request.HttpContext.Items[AbstractAspNetCoreFunction.LAMBDA_CONTEXT]);
            _logger.LogInformation("{@LambdaRequestObject}", Request.HttpContext.Items[AbstractAspNetCoreFunction.LAMBDA_REQUEST_OBJECT]);
            _logger.LogInformation("{@IdentityDetails}", GetIdentityDetails().ToList());

            var rideId = Guid.NewGuid().ToString();

            var username = User.Identity.Name;

            var unicorn = FindUnicorn(request.PickupLocation);

            await _dbContext.SaveAsync(new Ride
            {
                RideId = rideId,
                RequestTime = DateTime.UtcNow,
                Unicorn = unicorn,
                UnicornName = unicorn.Name,
                User = username
            });

            var response = new RideResponse
            {
                Eta = "30 seconds",
                RideId = rideId,
                Rider = username,
                Unicorn = unicorn,
                UnicornName = unicorn.Name
            };

            return CreatedAtAction(nameof(Post), response);
        }

        private Unicorn FindUnicorn(GeoLocation pickupLocation)
        {
            var r = new Random();

            _logger.LogInformation("Finding unicorn for {@PickupLocation}", pickupLocation);

            return _fleets[r.Next(0, _fleets.Length)];
        }

        private readonly Unicorn[] _fleets = new[]
        {
            new Unicorn{
                Name= "Bucephalus",
                Color= "Golden",
                Gender= "Male",
            },
            new Unicorn{
                Name= "Shadowfax",
                Color= "White",
                Gender= "Male",
            },
            new Unicorn{
                Name= "Rocinante",
                Color= "Yellow",
                Gender= "Female",
            },
        };

        public IEnumerable<string> GetIdentityDetails()
        {
            var identityDetails = new List<string>();
            var id = (ClaimsIdentity)User.Identity;

            identityDetails.Add($"Name: {id.Name}");
            identityDetails.Add($"AuthenticationType: {id.AuthenticationType}");
            identityDetails.Add($"IsAuthenticated: {id.IsAuthenticated}");
            identityDetails.Add($"NameClaimType: {id.NameClaimType}");
            identityDetails.Add($"RoleClaimType: {id.RoleClaimType}");

            return id.Claims.Select(c => $"{c.Type}: {c.Value}").Concat(identityDetails);
        }
    }
}
