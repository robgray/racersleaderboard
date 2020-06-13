using System;
using System.Collections.Generic;
using System.Text;
using RacersLeaderboard.Core.Services.iRacing;

namespace RacersLeaderboard.Core.Domain
{
    public class License : BaseEntity
    {
        public Guid DriverId { get; set; }
        public Driver Driver { get; set; }
        
        public CategoryEnum Category { get; set; }

        public int Starts { get; set; }

        public int Wins { get; set; }

        public int AvgStart { get; set; }

        public int AvgFinish { get; set; }

        public int AvgPointsPerRace { get; set; }

        public int Top25Percent { get; set; }

        public int AverageFieldSize { get; set; }

        public int Laps { get; set; }

        public int LapsLead { get; set; }

        public decimal AverageIncidents { get; set; }

        public string Class { get; set; }

        public string iRatingText { get; set; }

        public string ttRatingText { get; set; }

        public int iRating => iRatingText.IndexOf("--") == -1 ? int.Parse(iRatingText) : 0;

        public int ttRating => ttRatingText.IndexOf("--") == -1 ? int.Parse(ttRatingText) : 0;

        public string LicenseColor
        {
            get
            {
                if (Class.StartsWith("P"))
                    return Constants.LicenseColors.Pro;
                if (Class.StartsWith("A"))
                    return Constants.LicenseColors.A;
                if (Class.StartsWith("B"))
                    return Constants.LicenseColors.B;
                if (Class.StartsWith("C"))
                    return Constants.LicenseColors.C;
                if (Class.StartsWith("D"))
                    return Constants.LicenseColors.D;
                return Constants.LicenseColors.Rookie;
            }
        }
    }
}
