using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banking
{
    public class Stock
    {
        public Stock(Stock.Type stock)
        {
            Sector = stock;
        }

        public enum Type
        {
            Transportation,
            Energy,
            Financial,
            Services,
            Technology,
            Healthcare
        }

        public Type Sector;
        public float Amount;

        public float AdvanceMonth(float gainLossPercent)
        {
            float newAmount = Amount * (1 + gainLossPercent);
            float diff = newAmount - Amount;
            Amount = newAmount;
            return diff;
        }
    }
}
