using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using WildRydesWebApi.Entities;

namespace WildRydesWebApi.Extensions
{
    public static class DynamoDbServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamoDb(this IServiceCollection services)
        {
            // ReSharper disable once InconsistentNaming
            const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "RidesTable";

            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            var tableName = Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Ride)] = new Amazon.Util.TypeMapping(typeof(Ride), tableName);
            }

            services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddSingleton(sp => new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 });
            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };

            // TODO: singleton ??
            services.AddTransient<IDynamoDBContext>(sp => new DynamoDBContext(new AmazonDynamoDBClient(), config));

            return services;
        }
    }
}
