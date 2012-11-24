using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Banking
{
    public class Market
    {
        const float GLOBAL_TENDENCY = 0.002f;

        public static Market TheMarket = new Market();
        public static int Month = 0;

        int CurrentMonth = 0;
        public int CurrentYear = 2012;

        public Dictionary<Stock.Type, StockHistory> History = new Dictionary<Stock.Type, StockHistory>();

        Market()
        {
            History.Add(Stock.Type.Transportation, new StockHistory());
            History.Add(Stock.Type.Energy, new StockHistory());
            History.Add(Stock.Type.Financial, new StockHistory());
            History.Add(Stock.Type.Services, new StockHistory());
            History.Add(Stock.Type.Technology, new StockHistory());
            History.Add(Stock.Type.Healthcare, new StockHistory());
        }

        public void GenerateInitialHistory(int months)
        {
            for (int i = 0; i < months; ++i)
            {
                AdvanceMarket();
            }
        }

        public float GetValue(Stock.Type type, int month)
        {
            if (month > Month)
            {
                month = Month;
            }

            float value = 1.0f;
            for (int i = 0; i < month; ++i)
            {
                value *= 1.0f + History[type].History[i];
            }

            return value;
        }

        public void Set1m(Label label, Stock.Type type)
        {
            float value = History[type].History[Month - 1];
            label.Content = (value*100).ToString("F1") + "%";
            if (value >= 0)
            {
                label.Foreground = Brushes.Black;
            }
            else
            {
                label.Foreground = Brushes.Red;
            }
        }

        public void Set6m(Label label, Stock.Type type)
        {
            float final = 1.0f;
            for (int i = Month - 6; i < Month; ++i)
            {
                final *= History[type].History[i] + 1;
            }
            final -= 1.0f;
            label.Content = (final*100).ToString("F1") + "%";
            if (final >= 0)
            {
                label.Foreground = Brushes.Black;
            }
            else
            {
                label.Foreground = Brushes.Red;
            }
        }

        public void AdvanceMarket()
        {
            ++Month;

            History[Stock.Type.Transportation].Add(CalculateStockGainLoss());
            History[Stock.Type.Energy].Add(CalculateStockGainLoss());
            History[Stock.Type.Financial].Add(CalculateStockGainLoss());
            History[Stock.Type.Services].Add(CalculateStockGainLoss());
            History[Stock.Type.Technology].Add(CalculateStockGainLoss());
            History[Stock.Type.Healthcare].Add(CalculateStockGainLoss());
        }

        public void AdvanceCalendar()
        {
            ++CurrentMonth;
            if (CurrentMonth >= 12)
            {
                CurrentMonth = 0;
                CurrentYear++;
            }
        }

        public string GetCalendar()
        {
            string month = "Unknown";
            switch (CurrentMonth)
            {
                case 0: month = "January"; break;
                case 1: month = "February"; break;
                case 2: month = "March"; break;
                case 3: month = "April"; break;
                case 4: month = "May"; break;
                case 5: month = "June"; break;
                case 6: month = "July"; break;
                case 7: month = "August"; break;
                case 8: month = "September"; break;
                case 9: month = "October"; break;
                case 10: month = "November"; break;
                case 11: month = "December"; break;
            }
            return month + " " + CurrentYear.ToString();
        }

        public void Plot(LineGraph graph, Stock.Type type, int min, int max)
        {
            graph.Data.Clear();
            for (int i = min; i <= max; ++i)
            {
                float value = GetValue(type, i);
                graph.Data.Add(value);
            }
            graph.SetColor(0, 0, 0);
            graph.SetBaseline(1);
            graph.SetBaselineColor(128, 0, 0);
            graph.SetBackgroundColor(232, 255, 240);
            graph.ShowBaseline(false);
            graph.HideLegendMinMax();
            graph.SetLabel("24m");
            graph.Refresh();
        }

        public void PlotGlobal(LineGraph graph, int min, int max)
        {
            graph.Data.Clear();
            for (int i = min; i <= max; ++i)
            {
                float value = GetValue(Stock.Type.Transportation, i);
                value += GetValue(Stock.Type.Energy, i);
                value += GetValue(Stock.Type.Financial, i);
                value += GetValue(Stock.Type.Services, i);
                value += GetValue(Stock.Type.Technology, i);
                value += GetValue(Stock.Type.Healthcare, i);
                graph.Data.Add(value);
            }
            graph.SetColor(0, 0, 0);
            //graph.SetBaseline(6);
            graph.SetBaselineColor(128, 0, 0);
            graph.SetBackgroundColor(232, 255, 240);
            graph.ShowBaseline(false);
            graph.HideLegendMinMax();
            graph.SetLabel("All time");
            graph.Refresh();
        }

        float CalculateStockGainLoss()
        {
            // TODO: Use better math
            float gainLoss = Probability.Singleton.GetGaussian(0, 0.02f); // 2% std dev
            gainLoss += GLOBAL_TENDENCY;
            return gainLoss;
        }

    }
}
