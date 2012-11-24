using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banking
{
    public class StockHistory
    {
        public Stock.Type Sector;
        public List<float> History = new List<float>();

        public float GetGainLoss()
        {
            if (History.Count < 2)
            {
                return 0;
            }

            float latest = History[History.Count-1];
            //float previous = History[History.Count-2];
            //float diff = latest - previous;
            //float gainLoss = diff / previous;
            //return gainLoss;
            return latest;
        }

        public void Add(float value)
        {
            History.Add(value);
        }
    }
}
