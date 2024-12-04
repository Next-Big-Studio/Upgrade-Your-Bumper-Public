using System;
using System.Collections.Generic;
using Car;

namespace Saving_System
{
    [Serializable]
    public class LeaderboardData
    {
        public List<CarSystemData> cars;
        
        public LeaderboardData()
        {
            cars = new List<CarSystemData>();
        }
        
        public LeaderboardData(List<CarSystemData> cars)
        {
            this.cars = cars;
        }
        
        public LeaderboardData(CarLeaderboard carLeaderboard)
        {
            cars = new List<CarSystemData>();
            foreach (CarSystem car in carLeaderboard.carSystems)
            {
                cars.Add(new CarSystemData(car));
            }
        }
    }
}