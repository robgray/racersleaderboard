using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace RacersLeaderboard.Core.Services.iRacing
{
    public static class Constants
    {
        public static class Urls
        {
            public const string Base = "https://members.iracing.com/";
            public const string Login = "https://members.iracing.com/membersite/Login";
            public const string MembersiteHome = "http://members.iracing.com/membersite/member/Home.do";
            public const string DriverStatsData = "http://members.iracing.com/memberstats/member/DriverStatsData";
            public const string SeasonStandings = "http://members.iracing.com/memberstats/member/GetSeasonStandings";
            public const string SeasonTimeTrialStandings = "http://members.iracing.com/memberstats/member/GetSeasonTTStandings";
            // New Api Call added in 2020 Season 3 
            public const string SearchSeriesResults = "http://members.iracing.com/memberstats/member/SearchSeriesResults";
        }
        
        public static class Categories
        {
            public const int Oval = 1;
            public const int Road = 2;
            public const int DirtOval = 3;
            public const int DirtRoad = 4;
        }

        public static class ChartType
        {
            public const int iRating = 1;
            public const int ttRating = 2;
            public const int SafetyRating = 3;
        }

        public static class EventType
        {
            public const int Practice = 2;
            public const int Qualifying = 3;
            public const int TimeTrial = 4;
            public const int Race = 5;
        }

        public static class LicenseColors
        {
            public const string Pro = "000000";
            public const string A = "0153db";
            public const string B = "00c702";
            public const string C = "feec04";
            public const string D = "fc8a27";
            public const string Rookie = "fc0706";
        }
    }
}
