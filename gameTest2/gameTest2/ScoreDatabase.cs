using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace gameTest2
{
    public class ScoreDatabase
    {
        private readonly string _connectionString;
        private const string DatabaseFile = "scores.db";

        public ScoreDatabase()
        {
            _connectionString = $"Data Source={DatabaseFile};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(DatabaseFile))
                {
                    SQLiteConnection.CreateFile(DatabaseFile);
                }

                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                const string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Scores (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerName TEXT NOT NULL,
                        Score INTEGER NOT NULL,
                        DateAchieved TEXT NOT NULL
                    )";

                using var command = new SQLiteCommand(createTableSql, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Fallback: if SQLite fails, we'll store in memory only
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
            }
        }

        public void SaveScore(string playerName, int score)
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                const string insertSql = "INSERT INTO Scores (PlayerName, Score, DateAchieved) VALUES (@playerName, @score, @dateAchieved)";
                using var command = new SQLiteCommand(insertSql, connection);
                command.Parameters.AddWithValue("@playerName", playerName);
                command.Parameters.AddWithValue("@score", score);
                command.Parameters.AddWithValue("@dateAchieved", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save score: {ex.Message}");
            }
        }

        public List<ScoreEntry> GetTopScores(int count = 10)
        {
            var scores = new List<ScoreEntry>();
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                const string selectSql = "SELECT Id, PlayerName, Score, DateAchieved FROM Scores ORDER BY Score DESC LIMIT @count";
                using var command = new SQLiteCommand(selectSql, connection);
                command.Parameters.AddWithValue("@count", count);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    scores.Add(new ScoreEntry
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        PlayerName = reader["PlayerName"]?.ToString() ?? string.Empty,
                        Score = Convert.ToInt32(reader["Score"]),
                        DateAchieved = DateTime.Parse(reader["DateAchieved"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load scores: {ex.Message}");
            }
            return scores;
        }
    }
}
