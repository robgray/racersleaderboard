namespace RacersLeaderboard.Models.Simresults
{
    public class Lap
    {
        public string DriverName { get; set; }
        public string DriverGuid { get; set; }
        public int CarId { get; set; }
        public string CarModel { get; set; }
        public int TimeStamp { get; set; }
        public int LapTime { get; set; }
        public int[] Sectors { get; set; }
        public int Cuts { get; set; }
        public int BallastKg { get; set; }
    }
}
