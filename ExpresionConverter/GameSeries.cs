using System.Collections.Generic;

namespace ExpresionConverter
{
    public class GameSeries
    {
        [ListConvertable(typeof(DictionaryConverterDoubleTotal<GameMarkets>), typeof(List<GameStats>))]
        public List<GameMarkets> Markets { get; set; }

        [DictionarySource(SourceType = typeof(GameStats), SourcePropertyName = "TotalGames")]
        public Dictionary<string, double> TotalGames { get; set; }
    }
}