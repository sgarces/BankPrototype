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
using System.Windows.Shapes;

namespace Banking
{
    /// <summary>
    /// Interaction logic for Initial.xaml
    /// </summary>
    public partial class Initial : Window
    {
        Bank m_bank;

        public Initial(Bank bank)
        {
            InitializeComponent();
            sliderSetLoanInterest.Value = 4;
            sliderSetLoanRisk.Value = 0.2;
            sliderDepositsInterest.Value = 1;
            sliderBankingFees.Value = 5;
            m_bank = bank;
        }

        private void sliderSetLoanInterest_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBlockLoanInterest != null)
            {
                textBlockLoanInterest.Text = sliderSetLoanInterest.Value.ToString("F2") + " % per year";
            }
        }

        private void sliderSetLoanRisk_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBlockLoanRisk != null)
            {
                textBlockLoanRisk.Text = sliderSetLoanRisk.Value.ToString("F2") + " % defaults";
            }
        }

        private void sliderDepositsInterest_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBlockDepositInterest != null)
            {
                textBlockDepositInterest.Text = sliderDepositsInterest.Value.ToString("F2") + " % per year";
            }
        }

        private void sliderBankingFees_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBlockBankingFees != null)
            {
                textBlockBankingFees.Text = sliderBankingFees.Value.ToString("C") + " per month";
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m_bank.DepositBankingFee = (float)sliderBankingFees.Value;
            m_bank.DepositInterestRate = (float)sliderDepositsInterest.Value;
            m_bank.LoanInterestRate = (float)sliderSetLoanInterest.Value;
            m_bank.LoanRisk = (float)sliderSetLoanRisk.Value;
        }
    }
}
