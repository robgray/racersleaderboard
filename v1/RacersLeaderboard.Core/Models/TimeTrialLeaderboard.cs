using System.Collections.Generic;
using Newtonsoft.Json;

namespace RacersLeaderboard.Core.Models
{
    public class TimeTrialLeaderboard
    {
        public TimeTrialLeaderboard()
        {
            Data = new TimeTrialData();
        }

        [JsonProperty("d")]
        public TimeTrialData Data { get; set; }

        public class TimeTrialData
        {
            public TimeTrialData()
            {            
                TimeTrials = new List<TimeTrialItem>();
            }
            
            [JsonProperty("r")]
            public IList<TimeTrialItem> TimeTrials { get; set; }
        }

        public class TimeTrialItem
        {
            [JsonProperty("1")]
            public int Wins { get; set; }

            [JsonProperty("2")]
            public int Week { get; set; }

            [JsonProperty("3")]
            public int Rowcount { get; set; }

            [JsonProperty("4")]
            public int Dropped { get; set; }

            [JsonProperty("5")]
            public int HelmetPattern { get; set; }

            [JsonProperty("6")]
            public int MaxLicenseLevel { get; set; }

            [JsonProperty("7")]
            public int ClubId { get; set; }

            [JsonProperty("8")]
            public double Points { get; set; }

            [JsonProperty("9")]
            public int Division { get; set; }

            [JsonProperty("10")]
            public string HelmColor3 { get; set; }

            [JsonProperty("11")]
            public string ClubName { get; set; }

            [JsonProperty("12")]
            public string HelmColor1 { get; set; }

            [JsonProperty("13")]
            public string DisplayName { get; set; }

            [JsonProperty("14")]
            public string HelmColor2 { get; set; }

            [JsonProperty("15")]
            public int CustId { get; set; }

            [JsonProperty("16")]
            public string SubLevel { get; set; }

            [JsonProperty("17")]
            public int Rank { get; set; }

            [JsonProperty("18")]
            public int Position { get; set; }

            [JsonProperty("19")]
            public int Rn { get; set; }

            [JsonProperty("20")]
            public int Starts { get; set; }

            [JsonProperty("21")]
            public int CustRow { get; set; }
        }
    }
}