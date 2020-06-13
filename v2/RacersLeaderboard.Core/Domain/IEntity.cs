using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace RacersLeaderboard.Core.Domain
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}
