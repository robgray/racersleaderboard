using System;
using NodaTime;

namespace RacersLeaderboard.Core.Domain
{
    public class BaseEntity : IEntity, IHasAuditing
    {
        public Guid Id { get; set; }
        public Instant CreatedAt { get; set; }
        public Instant LastModified { get; set; }
    }
}
