using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExpresionConverter
{
    public class Program
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            // some source class with results
            var res1 = new GameStats
            {
                Kills = 12,
                Deaths = 2,
                Team1Won = true,
                TotalGames = 4,
            };

            var eventSource2 = new GameStats
            {
                TotalGames = 5,
            };

            // GameMarkets
            var gms = new GameMarkets
            {
                Kills = new Dictionary<double, Total>
                {
                    { 1, new Total { Under = 0.3, Over = 0.7 }},
                    { 2, new Total { Under = 0.4, Over = 0.6 }},
                    { 3, new Total { Under = 0.9, Over = 0.1 }}
                },
            };

            var series = new GameSeries {Markets = new List<GameMarkets> {gms, gms} ,
                TotalGames = new Dictionary<string, double>
                {
                    { "3", 0.2 },
                    { "4", 0.4 },
                    { "5",  0.6}
                }
            };

            //var result1 = JsonConvert.SerializeObject(gms, new DictionaryConverterDoubleTotal<GameMarkets>(res1));
            var result2 = JsonConvert.SerializeObject(gms, new DictionaryConverterStringDouble<GameSeries>(res1, new List<GameStats> { res1, res1 }));

            var result3 = JsonConvert.SerializeObject(series, new DictionaryConverterStringDouble<GameSeries>( new object[] { new List<GameStats> { res1, res1 } }));

            //var result = JsonConvert.SerializeObject(gms, new DictionaryConverter<GameMarkets>(eventSource1, eventSource2));
            //Console.WriteLine(result);
        }
    }
}