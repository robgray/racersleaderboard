using System;
using CsvHelper.Configuration;

namespace RacersLeaderboard.Core.Models
{
    
    public sealed class DriverStatsMap : ClassMap<DriverStats>
    {
        public DriverStatsMap()
        {
            Map(m => m.Pos).Index(0);
            Map(m => m.Driver).Index(1);
            Map(m => m.Location).Index(2);
            Map(m => m.Region).Index(3);
            Map(m => m.Club).Index(4);
            Map(m => m.Starts).Index(5);
            Map(m => m.Wins).Index(6);
            Map(m => m.AvgStart).Name("Avg Start").Index(7);
            Map(m => m.AvgFinish).Name("Avg Finish").Index(8);
            Map(m => m.AvgPointsPerRace).Name("Avg Pts per Race").Index(9);
            Map(m => m.Top25Percent).Name("Top 25%").Index(10);
            Map(m => m.AverageFieldSize).Name("Avg Field Size").Index(11);
            Map(m => m.Laps).Index(12);
            Map(m => m.LapsLead).Name("Laps Lead").Index(13);
            Map(m => m.AverageIncidents).Name("Avg Inc").Index(14);
            Map(m => m.Class).Index(15);
            Map(m => m.iRatingText).Name("iRating").Index(16);
            Map(m => m.ttRatingText).Name("ttRating").Index(17);
            Map(m => m.ClubPoints).Name("Club Pts").Index(18);
            Map(m => m.TotalPoints).Name("Total Pts").Index(19);
            Map(m => m.CustId).Index(20);
        }
    }

    public class DriverStats
    {
        public string Pos { get; set; }

        public int CustId { get; set; }

        public string Driver { get; set; }

        public string Location { get; set; }

        public string Region { get; set; }

        public string Club { get; set; }

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

        public int ClubPoints { get; set; }

        public int TotalPoints { get; set; }

        public decimal WinRate => (Wins/Convert.ToDecimal(Starts));

        public string LicenseColor
        {
            get
            {
                if (Class.StartsWith("P"))
                    return LicenseColors.Pro;
                if (Class.StartsWith("A"))
                    return LicenseColors.A;
                if (Class.StartsWith("B"))
                    return LicenseColors.B;
                if (Class.StartsWith("C"))
                    return LicenseColors.C;
                if (Class.StartsWith("D"))
                    return LicenseColors.D;

                return LicenseColors.Rookie;
            }
        }

        public string GetSignatureTemplate()
        {
            string signatureTemplate = "";
            if (Class.IndexOf("R ", StringComparison.Ordinal) != -1)
            {
                signatureTemplate = "signature-rookie.png";
            }
            else if (Class.IndexOf("D ", StringComparison.Ordinal) != -1)
            {
                signatureTemplate = "signature-d.png";
            }
            else if (Class.IndexOf("C ", StringComparison.Ordinal) != -1)
            {
                signatureTemplate = "signature-c.png";
            }
            else if (Class.IndexOf("B ", StringComparison.Ordinal) != -1)
            {
                signatureTemplate = "signature-b.png";
            }
            else if (Class.IndexOf("A ", StringComparison.Ordinal) != -1)
            {
                signatureTemplate = "signature-a.png";
            }
            else
            {
                signatureTemplate = "signature-pro.png";
            }

            return signatureTemplate;
        }


        public string LicenseColorForeground
        {
            get
            {
                if (Class.StartsWith("P") || Class.StartsWith("A") || Class.StartsWith("R"))
                    return "FFFFFF";

                return "000000";
            }
        }
    }
}