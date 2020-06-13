using System.Collections.Generic;
using RacersLeaderboard.Core.Services.iRacing.Models;

namespace RacersLeaderboard.Core.TableBuilders
{
    public interface IFluentDriverCharts
	{
        ImageCreator ForLeaderboard();
        ImageCreator ForStatsTable();
    }
	public interface IFluentSeasonStandingsCharts
    {
        ImageCreator ForStandings();
    }

    public interface IFluentTimeTrialCharts
    {
        ImageCreator ForTimeTrials();
    }
    
    public class FluentTableCreator
    {
        public IFluentDriverCharts WithDriverStats(List<DriverStats> driverStats)
        {
            return new FluentDriverCharts(driverStats);
        }

        public IFluentSeasonStandingsCharts WithSeasonStandings(List<SeasonStanding> seasonStandings)
        {
            return new FluentSeasonStandingsCharts(seasonStandings);
        }

        public IFluentTimeTrialCharts WithTimeTrials(List<TimeTrialLeaderboard.TimeTrialItem> timeTrials)
        {
            return new FluentTimeTrialCharts(timeTrials);
        }

        private class FluentTimeTrialCharts : IFluentTimeTrialCharts
        {
            private List<TimeTrialLeaderboard.TimeTrialItem> _timeTrials;
            public FluentTimeTrialCharts(List<TimeTrialLeaderboard.TimeTrialItem> timeTrials)
            {
                _timeTrials = timeTrials;
            }

            public ImageCreator ForTimeTrials()
            {
                return new TimeTrialTableBuilder(_timeTrials).Create();
            }
        }

        private class FluentDriverCharts : IFluentDriverCharts
        {
            private List<DriverStats> _driverStats;

            public FluentDriverCharts(List<DriverStats> driverStats)
            {
                _driverStats = driverStats;
            }

            public ImageCreator ForLeaderboard()
            {
                return new LeaderboardTableBuilder(_driverStats).Create();
            }

            public ImageCreator ForStatsTable()
            {
                return new StatisticsTableBuilder(_driverStats).Create();
            }
        }

        private class FluentSeasonStandingsCharts : IFluentSeasonStandingsCharts
        {
            private List<SeasonStanding> _standings;

            public FluentSeasonStandingsCharts(List<SeasonStanding> standings)
            {
                _standings = standings;
            }

            public ImageCreator ForStandings()
            {
                return new SeasonStandingTableBuilder(_standings).Create();
            }
        }

    }
}
