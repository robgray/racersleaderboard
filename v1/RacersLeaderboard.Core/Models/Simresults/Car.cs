namespace RacersLeaderboard.Core.Models.Simresults
{
    public class Car
    {
        public int CarId { get; set; }
        public Driver Driver { get; set; }
        public string Model { get; set; }
        public string Skin { get; set; }
        public int BallastKg { get; set; }
    }
}
