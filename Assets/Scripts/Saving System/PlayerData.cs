using System;

namespace Saving_System
{
    // All the car's data
    [Serializable]
    public class PlayerData
    {
        public int numberOfRaces;
        public int timesDestroyed ;
        public int timesSurvived;
        public int numberOfKills;
        public bool didFinish;
        public float raceTime;
        public int raceKills;
        public CarSystemData carSystemData = new();
        public GambleData gambleData;
    }
}