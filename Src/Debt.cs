using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banking
{
    public class Debt
    {
        public Bank Debtor;
        public float Principal;
        public float Interest; // monthly
        public int Months;

        public static Debt Create(Bank debtor, float amount, float interest, int months)
        {
            Debt d = new Debt();
            d.Debtor = debtor;
            d.Principal = amount;
            d.Interest = interest;
            d.Months = months;
            return d;
        }

        public float AdvanceMonth(out float payment)
        {
            if (IsFullyPaid())
            {
                payment = 0;
                return 0;
            }

            float interest = Principal * Interest / 100;
            payment = Loan.CalculateMonthlyPayment(Principal, Interest, Months);
            float capitalReturned = payment - interest;
            Principal -= capitalReturned;
            --Months;
            return interest;
        }

        public bool IsFullyPaid()
        {
            return Months <= 0;
        }
    }
}
