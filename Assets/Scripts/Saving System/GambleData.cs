using System;

namespace Saving_System
{
    [Serializable]
    public class GambleData 
    {
        public int timesGambled = 0;
        public int amountGambled = 0;
        public int amountWon = 0;
        public int amountLost = 0;
        public int timesWon = 0;
        public int timesLost = 0;
    }
}