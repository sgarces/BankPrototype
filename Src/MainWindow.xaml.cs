using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Banking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int initialCash = 10000000; // start with $10M
        public Bank MyBank = new Bank(initialCash);
        int stockBuySellAmount = 10000;
        DateTime m_startTime;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MyBank;
            m_startTime = DateTime.Now;
        }

        private void buttonAdvanceMonth_Click(object sender, RoutedEventArgs e)
        {
            if (RoundBorrowAmount() > 0 && RoundBorrowMonths() > 0)
            {
                MessageBoxResult result = MessageBox.Show("Seems like you meant to borrow money\nWould you like to still borrow this turn?", "Forgot to borrow?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    return;
                }
            }

            if (MyBank.Cash < 0)
            {
                MessageBoxResult result = MessageBox.Show("You have no cash! You're going to go BANKRUPT.\nDo you want to declare BANKRUPTCY?", "Bankrupt alert!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    // bankrupt
                    MessageBox.Show("You're BANKRUPT.\nYour assets are being liquidated.\nThe government decided not to bail you out.", "BANKRUPT", MessageBoxButton.OK, MessageBoxImage.Error);
                    buttonAdvanceMonth.IsEnabled = false;
                    return;
                }
                else
                {
                    return;
                }
            }

            PropagateInstructions();
            Market.TheMarket.AdvanceMarket();
            Market.TheMarket.AdvanceCalendar();
            MyBank.AdvanceMonth();
            UpdateData();
            if (Market.TheMarket.CurrentYear >= 2022)
            {
                int profit = (int)MyBank.GetTotalAssets() - initialCash;
                buttonAdvanceMonth.IsEnabled = false;
                TimeSpan span = DateTime.Now - m_startTime;
                string howlong = "";
                if (span.Hours > 0)
                {
                    howlong += span.Hours + " hr, ";
                }
                howlong += span.Minutes + " min, " + span.Seconds + " sec";
                if (profit > 0)
                {
                    MessageBox.Show("Your final profit is " + profit.ToString("C") + "\nTime played: " + howlong, "Time's Up", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
                }
                else
                {
                    MessageBox.Show("You LOST " + (-profit).ToString("C") + "\nTime played: " + howlong, "Time's Up", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
                }
            }

            sliderBorrowAmount.Value = 0;
            sliderBorrowPeriod.Value = 0;
        }

        public void UpdateData()
        {
            UpdateMonthlyResults();
            UpdateMarketsCharts();
            UpdateAssets();
            UpdateLoanInfo();
            UpdateDepositInfo();
            UpdateInvestments();
            UpdateBorrowAmount();
            UpdateBorrowPeriod();
            labelMonthYear.Content = Market.TheMarket.GetCalendar();
        }

        void UpdateMonthlyResults()
        {
            SetText(textMonthlyResults_BankingFees, MyBank.LastMonthBankingFees);
            SetText(textMonthlyResults_DepositsInterest, -MyBank.LastMonthDepositsInterest);
            SetText(textMonthlyResults_LoansInterest, MyBank.LastMonthLoansInterest);
            SetText(textMonthlyResults_NewDeposits, MyBank.LastMonthNewDeposits);
            //SetText(textMonthlyResults_Bonds, MyBank.LastMonthBondsGainLoss);
            SetText(textMonthlyResults_DebtInterest, -MyBank.LastMonthDebtInterest);
            //SetText(textMonthlyResults_InfrastructureCost, MyBank.LastMonthInfrastructureCost);
            //SetText(textMonthlyResults_StaffCost, MyBank.LastMonthStaffCost);
            SetText(textMonthlyResults_Stocks, MyBank.LastMonthStocksGainLoss);
            SetText(textMonthlyResults_LoansDefaulted, -MyBank.LastMonthLoansDefaulted);
            SetText(textMonthlyResults_NewLoans, MyBank.LastMonthNewLoans);
            SetText(textBoxInvestmentGainLoss, MyBank.LastMonthStocksGainLoss);
            SetText(textBoxAccInvestmentGainLoss, MyBank.AccumulatedInvestmentGainLoss);
            SetText(textMonthlyResults_Total, MyBank.LastMonthTotal);
        }

        void UpdateMarketsCharts()
        {
            int first = 0;
            int last = Market.Month;
            if (last - first > 24)
            {
                first = last - 24;
            }

            Market.TheMarket.Plot(lineGraph_Transportation, Stock.Type.Transportation, first, last);
            Market.TheMarket.Plot(lineGraph_Energy, Stock.Type.Energy, first, last);
            Market.TheMarket.Plot(lineGraph_Financial, Stock.Type.Financial, first, last);
            Market.TheMarket.Plot(lineGraph_Services, Stock.Type.Services, first, last);
            Market.TheMarket.Plot(lineGraph_Technology, Stock.Type.Technology, first, last);
            Market.TheMarket.Plot(lineGraph_Healthcare, Stock.Type.Healthcare, first, last);
            Market.TheMarket.PlotGlobal(lineGraphGlobalMarket, 0, last);

            Market.TheMarket.Set1m(label_Transportation_1m, Stock.Type.Transportation);
            Market.TheMarket.Set6m(label_Transportation_6m, Stock.Type.Transportation);
            Market.TheMarket.Set1m(label_Energy_1m, Stock.Type.Energy);
            Market.TheMarket.Set6m(label_Energy_6m, Stock.Type.Energy);
            Market.TheMarket.Set1m(label_Financial_1m, Stock.Type.Financial);
            Market.TheMarket.Set6m(label_Financial_6m, Stock.Type.Financial);
            Market.TheMarket.Set1m(label_Services_1m, Stock.Type.Services);
            Market.TheMarket.Set6m(label_Services_6m, Stock.Type.Services);
            Market.TheMarket.Set1m(label_Technology_1m, Stock.Type.Technology);
            Market.TheMarket.Set6m(label_Technology_6m, Stock.Type.Technology);
            Market.TheMarket.Set1m(label_Healthcare_1m, Stock.Type.Healthcare);
            Market.TheMarket.Set6m(label_Healthcare_6m, Stock.Type.Healthcare);
        }

        void UpdateLoanInfo()
        {
            //textSetLoanInterest.Text = MyBank.LoanInterestRate.ToString("F2");
            sliderSetLoanInterest.Value = MyBank.LoanInterestRate;
            UpdateLoanInterest();
            sliderSetLoanRisk.Value = MyBank.LoanRisk;
            UpdateRisk();
            UpdateLoansProjections();
        }

        void UpdateDepositInfo()
        {
            sliderDepositsInterest.Value = MyBank.DepositInterestRate;
            UpdateDepositsInterest();
            sliderBankingFees.Value = MyBank.DepositBankingFee;
            UpdateBankingFees();
            UpdateDepositsProjections();
        }

        void UpdateAssets()
        {
            SetText(textCash, MyBank.Cash);
            SetText(textCash2, MyBank.Cash);
            SetText(textLoans, MyBank.GetTotalMoneyLoaned());
            SetText(textLoans2, MyBank.GetTotalMoneyLoaned());
            SetText(textBoxLoanReserve, MyBank.LoanReserve);
            SetText(textDebt, MyBank.GetTotalMoneyBorrowed());
            SetText(textDebt2, -MyBank.GetTotalMoneyBorrowed());
            SetText(textDeposits, MyBank.GetTotalDeposits());
            SetText(textDeposits2, -MyBank.GetTotalDeposits());
            SetText(textStocks, MyBank.GetTotalStocks());
            SetText(textStocks2, MyBank.GetTotalStocks());
            SetText(textBoxTotalInvested, MyBank.GetTotalStocks());
            SetText(textTotalAssets2, MyBank.GetTotalAssets());
            SetText(textLoansInterest, MyBank.AccumulatedLoansInterest);
            SetText(textDebtInterest, MyBank.AccumulatedDebtInterest);
            SetText(textDepositsInterest, MyBank.AccumulatedDepositsInterest);
            SetText(textStocksGainLoss, MyBank.AccumulatedInvestmentGainLoss);

            SetText(textTotalHistoricalDefaults, MyBank.AccumulatedDefaults);

            MyBank.PlotHistory(lineGraphAssetHistory);
            MyBank.PlotCashHistory(lineGraphCash);
            MyBank.PlotStocksHistory(lineGraphStocks);
            MyBank.PlotStocksGainLossHistory(lineGraphInvestmentGainLoss);
            MyBank.PlotLoansHistory(lineGraphLoans);
            MyBank.PlotDebtHistory(lineGraphDebt);
            MyBank.PlotDepositsHistory(lineGraphDeposits);
            MyBank.PlotDepositsInterestHistory(lineGraphDepositsInterest);
            MyBank.PlotLoansInterestHistory(lineGraphLoansInterest);
            MyBank.PlotDebtInterestHistory(lineGraphDebtInterest);
            MyBank.PlotStocksGainLossHistory2(lineGraphStocksGainLoss);
            MyBank.PlotDefaultsHistory(lineGraphTotalDefaults);
        }

        void PropagateInstructions()
        {
            MyBank.LoanInterestRate = (float)sliderSetLoanInterest.Value;
            MyBank.LoanRisk = (float)sliderSetLoanRisk.Value;
            MyBank.DepositBankingFee = (float)sliderBankingFees.Value;
            MyBank.DepositInterestRate = (float)sliderDepositsInterest.Value;
        }

        //static float GetText(TextBox text)
        //{
        //    try
        //    {
        //        float f = Convert.ToSingle(text.Text);
        //        return f;
        //    }
        //    catch (FormatException)
        //    {
        //        return 0;
        //    }
        //}

        static void SetText(TextBox text, float f)
        {
            text.Text = ToText(f);
            if (f < 0)
            {
                text.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                text.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        static string ToText(float f)
        {
            return f.ToString("C");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Splash splash = new Splash();
            splash.ShowDialog();

            Initial initial = new Initial(MyBank);
            initial.ShowDialog();

            Market.TheMarket.GenerateInitialHistory(4 * 12); // 4 years of history

            labelMonthYear.Content = "January 2012";
            UpdateData();
        }

        #region Loans Page
        void UpdateDepositsProjections()
        {
            if (textNewDeposits == null)
            {
                return;
            }

            try
            {
                float interest = sliderDepositsInterest != null ? (float)sliderDepositsInterest.Value : 0;
                float fees = sliderBankingFees != null ? (float)sliderBankingFees.Value : 0;
                if (interest < 1e-5f)
                {
                    textNewDeposits.Text = "No deposits";
                }
                else
                {
                    float depositAvg = MyBank.CalculateNewDepositAverage(interest, fees);
                    int depositAvgInt = (int)(depositAvg * Bank.DEPOSIT_MEAN/1000)*1000;
                    textNewDeposits.Text = depositAvgInt.ToString("C");
                }
            }
            catch (FormatException)
            {
                textNewDeposits.Text = "No deposits";
            }
        }

        void UpdateLoansProjections()
        {
            if (textNewLoans == null)
            {
                return;
            }

            try
            {
                float interest = sliderSetLoanInterest != null ? (float)sliderSetLoanInterest.Value : 0;
                float risk = sliderSetLoanRisk != null ? (float)sliderSetLoanRisk.Value : 0;

                if (interest < 1e-5f || risk < 1e-5f)
                {
                    textNewLoans.Text = "No loans";
                }
                else
                {
                    float loanAvg = MyBank.CalculateNewLoanAverage(interest, risk);
                    int loanAvgInt = (int)(loanAvg * Bank.LOAN_MEAN/1000) * 1000;
                    textNewLoans.Text = loanAvgInt.ToString("C");
                }
            }
            catch (FormatException)
            {
                textNewLoans.Text = "No loans";
            }
        }

        #endregion

        private void UpdateLoanInterest()
        {
            if (textBlockLoanInterest != null)
            {
                textBlockLoanInterest.Text = sliderSetLoanInterest.Value.ToString("F2") + " % per year";
            }
        }
        private void UpdateRisk()
        {
            if (textBlockLoanRisk != null)
            {
                textBlockLoanRisk.Text = sliderSetLoanRisk.Value.ToString("F2") + " % defaults";
            }
        }
        private void UpdateDepositsInterest()
        {
            if (textBlockDepositInterest != null)
            {
                textBlockDepositInterest.Text = sliderDepositsInterest.Value.ToString("F2") + " % per year";
            }
        }
        private void UpdateBankingFees()
        {
            if (textBlockBankingFees != null)
            {
                textBlockBankingFees.Text = sliderBankingFees.Value.ToString("C") + " per month";
            }
        }

        private void sliderSetLoanInterest_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateLoanInterest();
            UpdateLoansProjections();
        }

        private void sliderSetLoanRisk_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateRisk();
            UpdateLoansProjections();
        }

        private void sliderDepositsInterest_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateDepositsInterest();
            UpdateDepositsProjections();
        }

        private void sliderBankingFees_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBankingFees();
            UpdateDepositsProjections();
        }

        void UpdateInvestments()
        {
            SetText(textBoxTransportation, MyBank.Stocks[Stock.Type.Transportation].Amount);
            SetText(textBoxEnergy, MyBank.Stocks[Stock.Type.Energy].Amount);
            SetText(textBoxHealthcare, MyBank.Stocks[Stock.Type.Healthcare].Amount);
            SetText(textBoxFinancial, MyBank.Stocks[Stock.Type.Financial].Amount);
            SetText(textBoxTechnology, MyBank.Stocks[Stock.Type.Technology].Amount);
            SetText(textBoxServices, MyBank.Stocks[Stock.Type.Services].Amount);
        }

        int GetBuyAmount()
        {
            return stockBuySellAmount;
        }

        private void buttonBuyTransportation_Click(object sender, RoutedEventArgs e)
        {
            MyBank.BuyStock(Stock.Type.Transportation, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonSellTransportation_Click(object sender, RoutedEventArgs e)
        {
            MyBank.SellStock(Stock.Type.Transportation, GetBuyAmount()); 
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonBuyEnergy_Click(object sender, RoutedEventArgs e)
        {
            MyBank.BuyStock(Stock.Type.Energy, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonSellEnergy_Click(object sender, RoutedEventArgs e)
        {
            MyBank.SellStock(Stock.Type.Energy, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonBuyFinancial_Click(object sender, RoutedEventArgs e)
        {
            MyBank.BuyStock(Stock.Type.Financial, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonSellFinancial_Click(object sender, RoutedEventArgs e)
        {
            MyBank.SellStock(Stock.Type.Financial, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonBuyServices_Click(object sender, RoutedEventArgs e)
        {
            MyBank.BuyStock(Stock.Type.Services, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonSellServices_Click(object sender, RoutedEventArgs e)
        {
            MyBank.SellStock(Stock.Type.Services, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonBuyTechnology_Click(object sender, RoutedEventArgs e)
        {
            MyBank.BuyStock(Stock.Type.Technology, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonSellTechnology_Click(object sender, RoutedEventArgs e)
        {
            MyBank.SellStock(Stock.Type.Technology, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonBuyHealthcare_Click(object sender, RoutedEventArgs e)
        {
            MyBank.BuyStock(Stock.Type.Healthcare, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void buttonSellHealthcare_Click(object sender, RoutedEventArgs e)
        {
            MyBank.SellStock(Stock.Type.Healthcare, GetBuyAmount());
            UpdateInvestments();
            UpdateAssets();
        }

        private void radioButton10K_Checked(object sender, RoutedEventArgs e)
        {
            stockBuySellAmount = 100000;
        }

        private void radioButton100K_Checked(object sender, RoutedEventArgs e)
        {
            stockBuySellAmount = 1000000;
        }

        private void radioButton500K_Checked(object sender, RoutedEventArgs e)
        {
            stockBuySellAmount = 10000000;
        }

        void UpdateBorrowInterest()
        {
            float amount = RoundBorrowAmount();
            int months = RoundBorrowMonths();
            bool canBorrow = amount > 0 && months > 0;
            if (textBlockBorrowInterest != null)
            {
                if (canBorrow)
                {
                    float interest = MyBank.CalculateBorrowInterest(months, amount);
                    textBlockBorrowInterest.Text = interest.ToString("F2") + " % per year";
                }
                else
                {
                    if (amount > 0)
                        textBlockBorrowInterest.Text = "Select months";
                    else
                        textBlockBorrowInterest.Text = "Select amount";
                }
            }
            if (buttonBorrowMoney != null)
            {
                if (canBorrow)
                {
                    buttonBorrowMoney.IsEnabled = true;
                }
                else
                {
                    buttonBorrowMoney.IsEnabled = false;
                }
            }
        }

        void UpdateBorrowPeriod()
        {
            if (textBlockBorrowPeriod != null)
            {
                int months = RoundBorrowMonths();
                textBlockBorrowPeriod.Text = months.ToString() + " months";
            }
            UpdateBorrowInterest();
        }

        int RoundBorrowMonths()
        {
            float months = (float)sliderBorrowPeriod.Value;
            int intMonths = (int) Math.Round(months);
            return intMonths;
        }

        int RoundBorrowAmount()
        {
            float amount = (float)sliderBorrowAmount.Value;
            int intAmount = (int)Math.Round(amount / 1000000) * 1000000;
            return intAmount;
        }

        void UpdateBorrowAmount()
        {
            if (textBlockBorrowAmount != null)
            {
                int amount = RoundBorrowAmount();
                textBlockBorrowAmount.Text = amount.ToString("C");
            }

            UpdateBorrowInterest();
        }

        private void sliderBorrowAmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBorrowAmount();
        }

        private void sliderBorrowPeriod_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBorrowPeriod();
        }

        private void borrowMoney_Click(object sender, RoutedEventArgs e)
        {
            MyBank.BorrowMoney(RoundBorrowMonths(), RoundBorrowAmount());
            UpdateData();
            sliderBorrowAmount.Value = 0;
            sliderBorrowPeriod.Value = 0;
        }

        private void buttonHelp_Click(object sender, RoutedEventArgs e)
        {
            Initial initial = new Initial(MyBank);
            initial.ShowDialog();
            UpdateData();
        }
    }
}
