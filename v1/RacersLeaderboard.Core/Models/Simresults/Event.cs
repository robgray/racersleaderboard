namespace RacersLeaderboard.Core.Models.Simresults
{
    public class Event
    {
        public string Type { get; set; }
        public int CarId { get; set; }
        public Driver Driver { get; set; }
        public int OtherCarId { get; set; }
        public Driver OtherDriver { get; set; }
        public float ImpactSpeed { get; set; }
        public Position WorldPosition { get; set; }
        public Position RelPosition { get; set; }
    }
}
