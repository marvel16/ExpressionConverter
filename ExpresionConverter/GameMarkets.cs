using System.Collections.Generic;

namespace ExpresionConverter
{
    public class GameMarkets
    {
        [DictionarySource(SourceType = typeof(GameStats), SourcePropertyName = "Kills")]
        public Dictionary<double, Total> Kills { get; set; }

        [BoolSource(SourceType = typeof(GameStats), SourcePropertyName = "Team1Won")]
        public double Team1ToWin { get; set;}
    }
}