namespace ExpresionConverter
{
    public class GameStats
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public bool Team1Won { get; set; }
        public int TotalGames { get; set; }

        public int HandicapsTeam1 { get; set; }

        public int NumberOfGames { get; set; }

        public string CorrectScore { get; set; }
    }
}