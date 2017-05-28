using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExpresionConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // some source class with results
            var res1 = new GameStats
            {
                Kills = 12,
                Deaths = 2,
                Team1Won = false,
                TotalGames = 4,
                HandicapsTeam1 = 2,
                NumberOfGames = 2,
                CorrectScore = "2-0",
            };

            
            var gms = new GameMarkets
            {
                Kills = new Dictionary<double, Total>
                {
                    { 1, new Total { Under = 0.3, Over = 0.7 }},
                    { 2, new Total { Under = 0.4, Over = 0.6 }},
                    { 3, new Total { Under = 0.9, Over = 0.1 }}
                },
            };

            var series = new SerieMarkets {BestOfOnes = new List<GameMarkets> {gms, gms} ,
                TotalGames = new Dictionary<string, double>
                {
                    { ">2.5", 0.2 },
                    { "<2.5", 0.4 },
                    { ">3.5", 0.2 },
                    { "<3.5", 0.4 },
                    { ">4.5",  0.6},
                    { "<4.5",  0.6}
                },

                NumberOfGames = new Dictionary<string, double>
                {
                    {"3", 1.16 },
                    {"4", 1.86 },
                    {"5", 1.05 },
                },

                HandicapsTeam1 = new Dictionary<string, double>
                {
                    {"+1.5", 1.16 },
                    { "-1.5", 1.86 },
                    {"+2.5", 1.05 },
                    {"-2.5", 3.88 },
                    {"+3.5", 1.05 },
                    {"-3.5", 3.88 },
                },
                
                CorrectScore = new Dictionary<string, double>
                {
                    {"3-0", 1.16 },
                    { "3-2", 1.86 },
                    { "2-3", 1.86 },
                    { "0-3", 1.86 },
                    { "1-3", 1.86 },
                    { "3-1", 1.86 },
                }
            };


            var result3 = JsonConvert.SerializeObject(series, new DictionaryConverter<SerieMarkets, string, double, object>(res1, new List<GameStats> { res1, res1 }));

            Console.ReadKey();
        }
    }
}