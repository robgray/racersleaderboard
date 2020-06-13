using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace RacersLeaderboard.Core.Domain
{
    public interface IHasAuditing
    {
        Instant CreatedAt { get; set; }
        Instant LastModified { get; set; }
    }
}
