using System;
using System.Collections.Generic;

namespace Saving_System
{
    // All the leaderboard's data
    [Serializable]
    public class GameData
    {
        public string dateStarted = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
        public int gameID = new Random().Next();
        public int difficulty = 1;
        public bool isFinished;
        public PlayerData playerData = new();
        public List<LeaderboardData> allRaces = new();
    }
}