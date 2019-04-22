using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace WildRydes
{
    public class Function
    {
        // This const is the name of the environment variable that the serverless.template will use to set
        // the name of the DynamoDB table used to store blog posts.
        // ReSharper disable once InconsistentNaming
        private const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "RidesTable";

        private readonly IDynamoDBContext _dbContext;

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

        public Function()
        {
            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            var tableName = Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Ride)] = new Amazon.Util.TypeMapping(typeof(Ride), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            _dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request.RequestContext.Authorizer == null)
            {
                return Error("Authorization not configured", context.AwsRequestId);
            }

            var rideId = Guid.NewGuid().ToString();

            context.Logger.LogLine($"Received request ({rideId}): {JsonConvert.SerializeObject(request)}");

            // Because we're using a Cognito User Pools authorizer, all of the claims
            // included in the authentication token are provided in the request context.
            // This includes the username as well as other attributes.
            var username = request.RequestContext.Authorizer.Claims["cognito:username"];

            // The body field of the event in a proxy integration is a raw string.
            // In order to extract meaningful values, we need to first parse this string
            // into an object. A more robust implementation might inspect the Content-Type
            // header first and use a different parsing strategy based on that value.
            var requestBody = JsonConvert.DeserializeObject<RideRequest>(request.Body);

            var pickupLocation = requestBody.PickupLocation;

            var unicorn = FindUnicorn(pickupLocation);

            try
            {
                await _dbContext.SaveAsync(new Ride
                {
                    RideId = rideId,
                    RequestTime = DateTime.UtcNow,
                    Unicorn = unicorn,
                    UnicornName = unicorn.Name,
                    User = username
                });

                var rider = new RideResponse
                {
                    Eta = "30 seconds",
                    RideId = rideId,
                    Rider = username,
                    Unicorn = unicorn,
                    UnicornName = unicorn.Name
                };

                return Created(rider);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Error(e.Message, context.AwsRequestId);
            }
        }

        // This is where you would implement logic to find the optimal unicorn for
        // this ride (possibly invoking another Lambda function as a microservice.)
        // For simplicity, we'll just pick a unicorn at random.
        private Unicorn FindUnicorn(GeoLocation pickupLocation)
        {
            var r = new Random();
            Console.WriteLine($"Finding unicorn for {pickupLocation.Latitude}, {pickupLocation.Longitude}");
            return _fleets[r.Next(0, _fleets.Length)];
        }

        private APIGatewayProxyResponse Error(string errorMessage, string awsRequestId)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = JsonConvert.SerializeObject(new { Error = errorMessage, Reference = awsRequestId }),
                Headers = new Dictionary<string, string>
                {
                    {"Access-Control-Allow-Origin", "*"}
                }
            };
        }

        private APIGatewayProxyResponse Created<T>(T data)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = JsonConvert.SerializeObject(data),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    {"Access-Control-Allow-Origin", "*"}
                }
            };
        }
    }
}
