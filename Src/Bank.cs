using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Banking
{
    public class Bank
    {
        public string Name;

        // position
        public float Cash = 0;
        public float LoanReserve = 0;
        public List<Loan> Loans = new List<Loan>();
        public List<Debt> Debts = new List<Debt>();
        public List<Deposit> Deposits = new List<Deposit>();
        public Dictionary<Stock.Type, Stock> Stocks = new Dictionary<Stock.Type, Stock>();

        float InitialCash = 0;

        // instructions
        public float LoanInterestRate = 0.01f;
        public float LoanRisk = 0.01f;
        public float DepositInterestRate = 0.01f;
        public float DepositBankingFee = 0;

        public List<float> CashHistory = new List<float>();
        public List<float> LoansHistory = new List<float>();
        public List<float> LoansInterestHistory = new List<float>();
        public List<float> DebtHistory = new List<float>();
        public List<float> DebtInterestHistory = new List<float>();
        public List<float> DepositsHistory = new List<float>();
        public List<float> DepositsInterestHistory = new List<float>();
        public List<float> StocksHistory = new List<float>();
        public List<float> StocksGainLossHistory = new List<float>();
        public List<float> AssetsHistory = new List<float>();
        public List<float> TotalDefaultsHistory = new List<float>();

        public float LastMonthLoansInterest;
        public float LastMonthDebtPayments;
        public float LastMonthDebtInterest;
        public float LastMonthCapitalReturned;
        public float LastMonthLoansDefaulted;
        public float LastMonthNewLoans;
        public float LastMonthBankingFees;
        public float LastMonthDepositsInterest;
        public float LastMonthNewDeposits;
        public float LastMonthStaffCost;
        public float LastMonthInfrastructureCost;
        public float LastMonthStocksGainLoss;
        public float LastMonthBondsGainLoss;

        public float AccumulatedInvestmentGainLoss;
        public float AccumulatedLoansInterest;
        public float AccumulatedDebtInterest;
        public float AccumulatedDepositsInterest;
        public float AccumulatedDefaults;


        const int ABSOLUTE_MAX_LOANS = 300;

        const float LOAN_RISK_HALF_MAX = 0.5f;
        const float LOAN_RISK_THREE_QUARTERS_MAX = 0.7f;
        const float LOAN_RISK_MULTIPLIER = 0.2f;

        const float LOAN_INTEREST_HALF_MAX = 3;

        const int ABSOLUTE_MAX_DEPOSITS = 1000;
        const float DEPOSIT_FEE_HALF_MAX = 5;
        const float DEPOSIT_INTEREST_HALF_MAX = 2.0f;
        const float DEPOSIT_INTEREST_THREE_QUARTERS_MAX = 2.5f;

        const float DEPOSIT_CANCEL_PROB = 1.0f / 100.0f; 

        public const int LOAN_MEAN = 200000; // $200K
        const int LOAN_STDEV = 100000; // +-$100K
        const int LOAN_DURATION = 15; // years
        const int LOAN_DURATION_STDEV = 5; // years

        public const int DEPOSIT_MEAN = 5000; // $5,000
        const int DEPOSIT_STDEV = 5000; // +- $5,000

        const float TARGET_PROFIT = 1.005f; // 0.5% monthly, 6% yearly

        const float PERCENT_LOSS_DEFAULT = 0.2f;

        const float LEVERAGE_INTEREST_HALF_MAX = 0.1f; // 5x
        const float LEVERAGE_INTEREST_THREE_QUARTERS_MAX = 0.25f; // 20x
        const float DEBT_INTEREST_MIN = 1;
        const float DEBT_INTEREST_MAX = 7;
        const float DEBT_MONTH_HALF_MAX = 12;
        const float DEBT_MONTH_ADD = 1;
        //const float DEBT_AMOUNT_HALF_MAX = 20000000;
        //const float DEBT_AMOUNT_ADD = 1;

        const float LOAN_RESERVE_LAW = 0.1f;
            

        public Bank(float cash)
        {
            Cash = cash;
            InitialCash = cash;
            UpdateHistory();
            Stocks[Stock.Type.Transportation] = new Stock(Stock.Type.Transportation);
            Stocks[Stock.Type.Energy] = new Stock(Stock.Type.Energy);
            Stocks[Stock.Type.Services] = new Stock(Stock.Type.Services);
            Stocks[Stock.Type.Healthcare] = new Stock(Stock.Type.Healthcare);
            Stocks[Stock.Type.Technology] = new Stock(Stock.Type.Technology);
            Stocks[Stock.Type.Financial] = new Stock(Stock.Type.Financial);
        }

        public float LastMonthTotal
        {
            get
            {
                return LastMonthLoansInterest
                    + LastMonthBankingFees
                    - LastMonthDepositsInterest
                    - LastMonthStaffCost
                    - LastMonthInfrastructureCost
                    + LastMonthStocksGainLoss
                    + LastMonthBondsGainLoss
                    - LastMonthLoansDefaulted
                    - LastMonthDebtInterest;
            }
        }


        public void AdvanceMonth()
        {
            // -- Loans
            LastMonthLoansInterest = 0;
            LastMonthCapitalReturned = 0;
            LastMonthLoansDefaulted = 0;
            foreach (Loan l in Loans)
            {
                bool newlyDefaulted = false;
                float capitalReturned = 0;
                LastMonthLoansInterest += l.AdvanceMonth(out newlyDefaulted, out capitalReturned);
                if (newlyDefaulted)
                {
                    // TODO: introduce randomness?
                    float capitalDefaulted = l.Principal * PERCENT_LOSS_DEFAULT;
                    LastMonthLoansDefaulted += capitalDefaulted;
                    LastMonthCapitalReturned += l.Principal - capitalDefaulted;
                    l.Principal = 0;
                }
                else
                {
                    LastMonthCapitalReturned += capitalReturned;
                }
            }
            Cash -= LastMonthLoansDefaulted;
            Cash += LastMonthLoansInterest;
            Cash += LastMonthCapitalReturned;
            AccumulatedLoansInterest += LastMonthLoansInterest;
            AccumulatedDefaults -= LastMonthLoansDefaulted;

            // -- Deposits
            LastMonthBankingFees = 0;
            LastMonthDepositsInterest = 0;
            foreach (Deposit d in Deposits)
            {
                float interest = 0;
                float fees = 0;
                d.AdvanceMonth(out interest, out fees);
                LastMonthBankingFees += fees;
                LastMonthDepositsInterest += interest;
            }
            Cash += LastMonthBankingFees;
            Cash -= LastMonthDepositsInterest;
            AccumulatedDepositsInterest -= LastMonthDepositsInterest;
            AccumulatedDepositsInterest += LastMonthBankingFees;

            // -- Debt
            LastMonthDebtPayments = 0;
            LastMonthDebtInterest = 0;
            foreach (Debt d in Debts)
            {
                float payment = 0;
                float interest = d.AdvanceMonth(out payment);
                LastMonthDebtPayments += payment;
                LastMonthDebtInterest += interest;
                Cash -= payment;
            }
            AccumulatedDebtInterest -= LastMonthDebtInterest;

            // -- Stock market
            // Done after the market is already updated
            float investmentGainLoss = 0;
            foreach (Stock s in Stocks.Values)
            {
                float gainLossPercent = Market.TheMarket.History[s.Sector].GetGainLoss();
                float gainLoss = s.AdvanceMonth(gainLossPercent);
                investmentGainLoss += gainLoss;
            }
            LastMonthStocksGainLoss = investmentGainLoss;
            AccumulatedInvestmentGainLoss += investmentGainLoss;

            AddNewDeposits();
            AddNewLoans();

            float loanReserveRequired = GetTotalMoneyLoaned() * LOAN_RESERVE_LAW;
            Cash += LoanReserve - loanReserveRequired;
            LoanReserve = loanReserveRequired;

            UpdateHistory();
        }

        void UpdateHistory()
        {
            CashHistory.Add(Cash);
            LoansHistory.Add(GetTotalMoneyLoaned());
            DebtHistory.Add(GetTotalMoneyBorrowed());
            DepositsHistory.Add(GetTotalDeposits());
            StocksHistory.Add(GetTotalStocks());
            StocksGainLossHistory.Add(AccumulatedInvestmentGainLoss);
            AssetsHistory.Add(GetTotalAssets());
            DepositsInterestHistory.Add(AccumulatedDepositsInterest);
            DebtInterestHistory.Add(AccumulatedDebtInterest);
            LoansInterestHistory.Add(AccumulatedLoansInterest);
            TotalDefaultsHistory.Add(AccumulatedDefaults);
        }

        public float GetTotalAssets()
        {
            float totalAssets = Cash;
            totalAssets += LoanReserve;
            totalAssets += GetTotalMoneyLoaned();
            totalAssets -= GetTotalDeposits();
            totalAssets += GetTotalStocks();
            totalAssets -= GetTotalMoneyBorrowed();
            return totalAssets;
        }

        public float GetTotalMoneyBorrowed()
        {
            float total = 0;
            foreach (Debt d in Debts)
            {
                total += d.Principal;
            }
            return total;
        }

        public float GetTotalMoneyLoaned()
        {
            float total = 0;
            foreach (Loan l in Loans)
            {
                if (!l.Defaulted)
                {
                    total += l.Principal;
                }
            }
            return total;
        }

        public float GetTotalHistoricalDefaults()
        {
            float total = 0;
            foreach (Loan l in Loans)
            {
                if (l.Defaulted)
                {
                    total += l.Principal;
                }
            }
            return total;
        }

        public float GetTotalDeposits()
        {
            float total = 0;
            foreach (Deposit d in Deposits)
            {
                total += d.Amount;
            }
            return total;
        }

        public float GetTotalStocks()
        {
            float total = 0;
            foreach (Stock s in Stocks.Values)
            {
                total += s.Amount;
            }
            return total;
        }

        void AddNewDeposits()
        {
            LastMonthNewDeposits = 0;

            // First cancel some existing deposits
            // What's the logic for this?
            // Reputation? Credit number?
            // Cash? -> positive feedback for bank runs
            int canceled = 0;
            for (int i = 0; i < Deposits.Count; ++i)
            {
                float cancel = Probability.Singleton.GetUniformFloat(0.0f, 1.0f);
                if (cancel < DEPOSIT_CANCEL_PROB)
                {
                    LastMonthNewDeposits -= Deposits[i].Amount;
                    Cash -= Deposits[i].Amount;
                    Deposits.RemoveAt(i);
                    --i;
                    ++canceled;
                }
            }

            //System.Diagnostics.Debug.WriteLine("Cancelled: " + canceled);
            
            // Only allow new deposits if values are set properly
            if (DepositInterestRate > 1e-5f)  // banking fees can be $0
            {
                // New deposits are determined by
                // - Interest set
                // - Banking fees
                // - Staffing - modifier/scaler
                float depositAvg = CalculateNewDepositAverage(DepositInterestRate, DepositBankingFee);
                int numDeposits = Probability.Singleton.GetPoisson(depositAvg);
                for (int i = 0; i < numDeposits; ++i)
                {
                    float amount = Probability.Singleton.GetGaussian(DEPOSIT_MEAN, DEPOSIT_STDEV);
                    amount = Math.Max(amount, 100);
                    Cash += amount;
                    LastMonthNewDeposits += amount;
                    Deposit d = new Deposit();
                    d.Amount = amount;
                    d.Interest = DepositInterestRate / 12.0f;
                    d.MonthlyFee = DepositBankingFee;
                    Deposits.Add(d);
                }
            }

        }

        public float CalculateNewDepositAverage(float interest, float fees)
        {
            // FIXME: I'm using these functions as CDF (cumulative distribution function)
            // Math will have to be reviewed

            //float interest_modifier = Probability.Singleton.InvExpDecreasing(interest, DEPOSIT_INTEREST_HALF_MAX);
            float interest_modifier = Probability.Singleton.Sigmoid(interest, DEPOSIT_INTEREST_HALF_MAX, DEPOSIT_INTEREST_THREE_QUARTERS_MAX);
            float fees_modifier = Probability.Singleton.InvExpDecreasing(fees, DEPOSIT_FEE_HALF_MAX);

            // Both interest and fee modifier are values between 0 and 1
            // 1 would indicate get max deposits, 0 means get nothing
            // They're independent, so we multiply them together for the final result
            float loanAvg = ABSOLUTE_MAX_DEPOSITS * interest_modifier * fees_modifier;
            return loanAvg;
        }

        bool HaveMoneyToLend(float amount)
        {
            float totalLoans = GetTotalMoneyLoaned() + amount;
            float totalCash = Cash + LoanReserve;
            totalCash -= totalLoans * LOAN_RESERVE_LAW;
            totalCash -= amount;
            return totalCash > 0;
        }

        float CalculateRiskFactor(float risk)
        {
            float riskFactor = Probability.Singleton.Sigmoid(risk, LOAN_RISK_HALF_MAX, LOAN_RISK_THREE_QUARTERS_MAX);
            riskFactor *= LOAN_RISK_MULTIPLIER * risk;
            return riskFactor;
        }

        void AddNewLoans()
        {
            LastMonthNewLoans = 0;
            // Only allow new loans if values are set properly
            if (LoanInterestRate > 1e-5f && LoanRisk > 1e-5f)
            {
                // New Loans are determined by:
                // - Available money - limit
                // - Interest rate set - follows certain prob curve
                // - Risk tolerance
                // - Staffing - modifier/scaler

                float loanAvg = CalculateNewLoanAverage(LoanInterestRate, LoanRisk);
                float maxRisk = Probability.Singleton.Sigmoid(LoanRisk, LOAN_RISK_HALF_MAX, LOAN_RISK_THREE_QUARTERS_MAX);
                

                int numLoans = Probability.Singleton.GetPoisson(loanAvg);
                for (int i = 0; i < numLoans; ++i)
                {
                    float amount = Probability.Singleton.GetGaussian(LOAN_MEAN, LOAN_STDEV);
                    amount = Math.Max(amount, 5000);
                    if (!HaveMoneyToLend(amount))
                    {
                        MessageBox.Show("You've run out of money to lend", "Not enough cash", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        break;
                    }
                    int duration = (int) Probability.Singleton.GetGaussian(LOAN_DURATION, LOAN_DURATION_STDEV);
                    float risk = Probability.Singleton.GetSigmoid(maxRisk, LOAN_RISK_HALF_MAX, LOAN_RISK_THREE_QUARTERS_MAX);
                    Cash -= amount;
                    LastMonthNewLoans += amount;
                    Loan l = Loan.Create(this, amount, LoanInterestRate / 12.0f, 12 * duration, risk);
                    Loans.Add(l);
                }
            }
        }

        public float CalculateNewLoanAverage(float interest, float risk)
        {
            // FIXME: I'm using these functions as CDF (cumulative distribution function)
            // Math will have to be reviewed

            float interest_modifier = Probability.Singleton.InvExpDecreasing(interest, LOAN_INTEREST_HALF_MAX);
            float risk_modifier = CalculateRiskFactor(risk);

            // Both interest and risk modifier are values between 0 and 1
            // 1 would indicate get max loan, 0 means get nothing
            // They're independent, so we multiply them together for the final result
            float loanAvg = ABSOLUTE_MAX_LOANS * interest_modifier * risk_modifier;
            return loanAvg;
        }

        public float CalculateBorrowInterest(float months, float amount)
        {
            float assets = GetTotalAssets();
            if (Math.Abs(assets) < 1)
            {
                assets = Math.Sign(assets);
            }
            float borrowed = GetTotalMoneyBorrowed();
            borrowed += amount;
            if (borrowed < 1e-5f)
            {
                borrowed = 1;
            }
            float leverage = assets / borrowed;
            float interest = Probability.Singleton.Sigmoid(leverage, LEVERAGE_INTEREST_HALF_MAX, LEVERAGE_INTEREST_THREE_QUARTERS_MAX);
            interest = 1 - interest;
            interest *= (DEBT_INTEREST_MAX - DEBT_INTEREST_MIN);
            interest += DEBT_INTEREST_MIN;
            float monthOffset = Probability.Singleton.InvExp(months, DEBT_MONTH_HALF_MAX);
            monthOffset *= DEBT_MONTH_ADD;
            interest += monthOffset;
            //float amountOffset = Probability.Singleton.InvExp(amount, DEBT_AMOUNT_HALF_MAX);
            //amountOffset *= DEBT_AMOUNT_ADD;
            //interest += amountOffset;
            return interest;
        }

        public void BorrowMoney(float months, float amount)
        {
            float interest = CalculateBorrowInterest(months, amount);
            Debt d = Debt.Create(this, amount, interest/12.0f, (int) months);
            Debts.Add(d);
            Cash += amount;
        }

        public void BuyStock(Stock.Type stock, int buy)
        {
            float actualBuy = Math.Min(buy, Cash);
            Cash -= actualBuy;
            Stocks[stock].Amount += actualBuy;
        }

        public void SellStock(Stock.Type stock, int sell)
        {
            float actualSell = Math.Min(Stocks[stock].Amount, sell);
            Cash += actualSell;
            Stocks[stock].Amount -= actualSell;
        }


        public void PlotHistory(LineGraph graph)
        {
            float target = AssetsHistory[0];

            graph.Data.Clear();
            graph.Target.Clear();
            for (int i = 0; i < AssetsHistory.Count; ++i)
            {
                graph.Data.Add(AssetsHistory[i]);
                graph.Target.Add(target);
                target *= TARGET_PROFIT;
            }
            graph.SetColor(0, 0, 0);
            graph.SetBackgroundColor(255, 255, 232);
            graph.SetBaseline(InitialCash, InitialCash - 100000, InitialCash + 100000);
            graph.SetLabel("Evolution");
            graph.Refresh();
        }

        void PlotHistoryInternal(LineGraph graph, List<float> history)
        {
            graph.Data.Clear();
            graph.Target.Clear();
            for (int i = 0; i < history.Count; ++i)
            {
                graph.Data.Add(history[i]);
            }
            graph.SetColor(0, 0, 0);
            graph.SetBackgroundColor(216, 255, 216);
            graph.SetBaseline(0);
            graph.SetLabel("Evolution");
            graph.Refresh();
        }
        public void PlotCashHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, CashHistory);
        }
        public void PlotStocksHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, StocksHistory);
        }
        public void PlotStocksGainLossHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, StocksGainLossHistory);
            graph.SetBackgroundColor(232, 255, 240);
        }
        public void PlotStocksGainLossHistory2(LineGraph graph)
        {
            PlotHistoryInternal(graph, StocksGainLossHistory);
        }
        public void PlotLoansHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, LoansHistory);
        }
        public void PlotDebtHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, DebtHistory);
        }
        public void PlotDepositsHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, DepositsHistory);
        }
        public void PlotDepositsInterestHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, DepositsInterestHistory);
        }
        public void PlotLoansInterestHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, LoansInterestHistory);
        }
        public void PlotDebtInterestHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, DebtInterestHistory);
        }
        public void PlotDefaultsHistory(LineGraph graph)
        {
            PlotHistoryInternal(graph, TotalDefaultsHistory);
        }
    }
}
