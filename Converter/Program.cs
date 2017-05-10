using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Converter
{
    public class Total
    {
        public double Under { get; set; }
        public double Over { get; set; }
    }

    public class EventSource1
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public bool Team1Won { get; set; }
    }

    public class EventSource2
    {
        public bool TokenTaken { get; set; }
    }

    public class MyClass
    {
        [DictionarySource(SourceType = typeof(EventSource1), SourcePropertyName = "Kills")]
        public Dictionary<double, Total> Kills { get; set; }

        [DictionarySource(SourceType = typeof(EventSource1), SourcePropertyName = "Deaths")]
        public Dictionary<double, Total> Deaths { get; set; }

        [BoolSource(SourceType = typeof(EventSource1), SourcePropertyName = "Team1Won")]
        public double Team1ToWin { get; set;}

        [BoolSource(SourceType = typeof(EventSource2), SourcePropertyName = "TokenTaken")]
        public double TokenTakenProbability { get; set; }
    }


    public class Program
    {
        public static void Main(string[] args)
        {
            // some source class with results
            var eventSource1 = new EventSource1
            {
                Kills = 12,
                Deaths = 2,
                Team1Won = true
            };

            var eventSource2 = new EventSource2 { TokenTaken  = true };

            // GameMarkets
            var mc = new MyClass
            {
                Kills = new Dictionary<double, Total>
                {
                    { 1, new Total { Under = 0.3, Over = 0.7 }},
                    { 2, new Total { Under = 0.4, Over = 0.6 }},
                    { 3, new Total { Under = 0.9, Over = 0.1 }}
                },
                Deaths = new Dictionary<double, Total>
                {
                    { 1, new Total { Under = 0.3, Over = 0.7 }},
                    { 2, new Total { Under = 0.4, Over = 0.6 }},
                    { 3, new Total { Under = 0.9, Over = 0.1 }}
                }
            };

            var result = JsonConvert.SerializeObject(mc, new DictionaryConverter<MyClass>(eventSource1, eventSource2));
            Console.WriteLine(result);
        }
    }
}
