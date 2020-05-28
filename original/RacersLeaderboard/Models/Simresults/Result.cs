namespace RacersLeaderboard.Models.Simresults
{
    public class Result
    {
        public string DriverName { get; set; }
        public string DriverGuid { get; set; }
        public int CarId { get; set; }
        public string CarModel { get; set; }
        public int BestLap { get; set; }
        public int TotalTime { get; set; }
        public int BallastKg { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }        
        public string TeamName { get; set; }
    }
}
