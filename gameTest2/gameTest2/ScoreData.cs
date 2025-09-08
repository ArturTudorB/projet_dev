using System;

namespace gameTest2
{
    public class ScoreEntry
    {
        public int Id { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime DateAchieved { get; set; }
    }
}
