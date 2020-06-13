using System;
using System.Collections.Generic;
using System.Linq;

namespace RacersLeaderboard.Core.Domain
{
    public class Driver : BaseEntity
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }

        public Guid? TeamId { get; set; }
        public Team Team { get; set; }

        public string Location { get; set; }
        public string Region { get; set; }
        public string Club { get; set; }

        public ICollection<License> Licenses { get; set; }
        public License RoadLicense => Licenses.FirstOrDefault(license => license.Category == CategoryEnum.Road);
        public License OvalLicense => Licenses.FirstOrDefault(license => license.Category == CategoryEnum.Oval);
        public License DirtOvalLicense => Licenses.FirstOrDefault(license => license.Category == CategoryEnum.DirtOval);
        public License DirtRoadLicense => Licenses.FirstOrDefault(license => license.Category == CategoryEnum.DirtRoad);
    }
}
