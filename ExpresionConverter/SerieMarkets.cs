using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ExpresionConverter
{
    public class SerieMarkets
    {
        public long EventId { get; set; }

        [DictionaryHandicaps(SourceType = typeof(GameStats), SourcePropertyName = nameof(GameStats.HandicapsTeam1))]
        public Dictionary<string, double> HandicapsTeam1 { get; set; }

        [DictionaryRange(SourceType = typeof(GameStats), SourcePropertyName = nameof(GameStats.TotalGames))]
        public Dictionary<string, double> TotalGames { get; set; }

        [DictionaryExact(SourceType = typeof(GameStats), SourcePropertyName = nameof(GameStats.NumberOfGames))]
        public Dictionary<string, double> NumberOfGames { get; set; }

        [DictionaryExactString(SourceType = typeof(GameStats), SourcePropertyName = nameof(GameStats.CorrectScore))]
        public Dictionary<string, double> CorrectScore { get; set; }

        [ListConvertable(typeof(DictionaryConverter<GameMarkets, double, Total, double>), typeof(List<GameStats>))]
        public List<GameMarkets> BestOfOnes { get; set; }
    }
}