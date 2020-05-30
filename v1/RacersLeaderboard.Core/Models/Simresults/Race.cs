using System.Collections.Generic;

namespace RacersLeaderboard.Core.Models.Simresults
{
    public class Race
    {
        public string TrackName { get; set; }
        public string TrackConfig { get; set; }
        public string Type { get; set; }
        public int DurationSecs { get; set; }
        public int RaceLaps { get; set; }
        public List<Car> Cars { get; set; }
        public List<Result> Result { get; set; }
        public List<Lap> Laps { get; set; }
        public List<Event> Events { get; set; }

    }
}
