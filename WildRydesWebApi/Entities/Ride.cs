using System;
using Amazon.DynamoDBv2.DataModel;

namespace WildRydesWebApi.Entities
{
    public class Ride
    {
        [DynamoDBHashKey]
        public string RideId { get; set; }
        public string User { get; set; }
        public Unicorn Unicorn { get; set; }
        public string UnicornName { get; set; }
        public DateTime RequestTime { get; set; }
    }
}