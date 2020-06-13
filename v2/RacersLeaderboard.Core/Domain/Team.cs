using System;
using System.Collections.Generic;
using System.Text;

namespace RacersLeaderboard.Core.Domain
{
    public class Team : BaseEntity
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public ICollection<Signature> Signatures { get; set; }
        public ICollection<Driver> Drivers { get; set; }
    }
}
