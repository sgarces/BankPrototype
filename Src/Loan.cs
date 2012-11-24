using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banking
{
    public class Loan
    {
        static Random RiskRNG = new Random();

        public Bank Loaner;
        public float Principal;
        public float Interest; // monthly
        public int Months;
        public float DefaultRisk;
        public bool Defaulted;

        public static Loan Create(Bank bank, float amount, float interest, int months, float risk)
        {
            Loan l = new Loan();
            l.Loaner = bank;
            l.Principal = amount;
            l.Interest = interest;
            l.Months = months;
            l.DefaultRisk = risk;
            return l;
        }

        public float AdvanceMonth(out bool newlyDefaulted, out float capitalReturned)
        {
            if (IsFullyPaid())
            {
                newlyDefaulted = false;
                capitalReturned = 0;
                return 0;
            }

            newlyDefaulted = ApplyDefaultRisk();

            if (Defaulted)
            {
                capitalReturned = 0;
                return 0;
            }

            float interest = Principal * Interest / 100;
            float payment = CalculateMonthlyPayment(Principal, Interest, Months);
            capitalReturned = payment - interest;
            Principal -= capitalReturned;
            --Months;
            return interest;
        }

        public bool IsFullyPaid()
        {
            return Months <= 0;
        }

        public bool HasDefaulted()
        {
            return Defaulted;
        }

        static public float CalculateMonthlyPayment(float principal, float interest, int months)
        {
            // P = L * (c * (1+c)^n) / ((1+c)^n - 1)
            double i = interest / 100;
            double payment = i + i / (Math.Pow(1 + i, months) - 1);
            payment *= principal;
            //float OnePlusC = 1 + Interest / 100;
            //float OnePlusCN = (float) Math.Pow(OnePlusC, Months);

            //float payment = Principal * Interest / 100 * OnePlusCN / (OnePlusCN - 1);
            return (float) payment;
        }

        bool ApplyDefaultRisk()
        {
            // NOTE: This math means that in aggregate the
            // randomness will be gaussian
            // Need to figure out how to make it more fractal
            if (!Defaulted)
            {
                float defaultChance = (float)RiskRNG.NextDouble();
                defaultChance *= 100;
                Defaulted = defaultChance < DefaultRisk;

                return Defaulted;
            }
            else
            {
                // already defaulted - don't count it again
                return false;
            }
        }
    }
}
