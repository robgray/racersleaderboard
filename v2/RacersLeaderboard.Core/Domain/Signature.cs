using System;

namespace RacersLeaderboard.Core.Domain
{
    public class Signature : BaseEntity
    {
        public string Filename { get; set; }
        public License ForLicense { get; set; } 
        public bool IsDefault { get; set; }

        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        
    }
}