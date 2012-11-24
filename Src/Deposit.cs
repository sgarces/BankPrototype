using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banking
{
    public class Deposit
    {
        public float Amount;
        public float Interest;
        public float MonthlyFee;

        public void AdvanceMonth(out float interest, out float fee)
        {
            interest = Amount * Interest / 100;
            fee = MonthlyFee;

            if (Amount > 0)
                Amount += interest;
            else
                interest = 0;
            Amount -= MonthlyFee;
        }
    }
}
