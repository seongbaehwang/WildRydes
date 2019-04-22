using WildRydesWebApi.Entities;

namespace WildRydesWebApi.Models
{
    public class RideResponse
    {
        public string RideId { get; set; }
        public string Rider { get; set; }
        public Unicorn Unicorn { get; set; }
        public string UnicornName { get; set; }
        public string Eta { get; set; }
    }
}