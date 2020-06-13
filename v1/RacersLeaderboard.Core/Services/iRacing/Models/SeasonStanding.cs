using CsvHelper.Configuration;

namespace RacersLeaderboard.Core.Services.iRacing.Models
{
	public class SeasonStandingMap : ClassMap<SeasonStanding>
	{
		public SeasonStandingMap()
		{
			Map(m => m.Position).Index(0).Name("position");
			Map(m => m.Name).Index(1).Name("name");
			Map(m => m.Points).Index(2).Name("points");
			Map(m => m.Dropped).Index(3).Name("dropped");
			Map(m => m.ClubName).Index(4).Name("clubname");
			Map(m => m.CountryCode).Index(5).Name("countrycode");
			Map(m => m.iRating).Index(6).Name("irating");
			Map(m => m.AvgFinish).Index(7).Name("avgfinish");			
			Map(m => m.TopFive).Index(8).Name("topfive");
			Map(m => m.Starts).Index(9).Name("starts");
			Map(m => m.LapsLead).Index(10).Name("lapslead");
			Map(m => m.Wins).Index(11).Name("wins");
			Map(m => m.Incidents).Index(12).Name("incidents");
			Map(m => m.Division).Index(13).Name("division");
			Map(m => m.WeeksCounted).Index(14).Name("weekscounted");
			Map(m => m.Laps).Index(15).Name("laps");
			Map(m => m.Poles).Index(16).Name("poles");
			Map(m => m.AvgStart).Index(17).Name("avgstart");
			Map(m => m.CustId).Index(18).Name("custid");
		}
	}

	public class SeasonStanding
	{
		public int Position { get; set; }

		public int CustId { get; set; }

		public string Name { get; set; }

		public int Points { get; set; }

		public string ClubName { get; set; }

		public string CountryCode { get; set; }

		public int iRating { get; set; }

		public int Dropped { get; set; }

		public int AvgFinish { get; set; }

		public int TopFive { get; set; }

		public int Starts { get; set; }

		public int LapsLead { get; set; }

		public int Wins { get; set; }

		public int Incidents { get; set; }

		public int Division { get; set; }

		public int WeeksCounted { get; set; }

		public int Laps { get; set; }

		public int Poles { get; set; }

		public int AvgStart { get; set; }

		public int TotalDrivers { get; set; }

        public int DriversInDivision { get; set; }

        public int DivisionPosition { get; set; }

	}
}