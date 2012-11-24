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
    public partial class LineGraph : UserControl
    {
        public List<double> Data = new List<double>();
        public List<double> Target = new List<double>();
        public double MinValue = 1e20;
        public double MaxValue = -1e20;

        SolidColorBrush m_graphColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        SolidColorBrush m_targetColor = new SolidColorBrush(Color.FromRgb(220, 200, 128));
        SolidColorBrush m_zeroLineColor = new SolidColorBrush(Color.FromRgb(150, 0, 0));
        double m_zeroLineValue = 0;
        Polyline m_polyline;
        Polyline m_targetPolyline;
        Line m_line;
        Point[] m_points;
        Point[] m_zeroline;
        Point[] m_targetline;
        bool m_showBaseline = true;

        public LineGraph()
        {
            InitializeComponent();
            m_polyline = new Polyline();
            m_targetPolyline = new Polyline();
            m_line = new Line();
            canvas1.Children.Add(m_polyline);
            canvas1.Children.Add(m_targetPolyline);
            canvas1.Children.Add(m_line);
            m_zeroline = new Point[2];
        }

        public void SetLabel(string label)
        {
            label1.Content = label;
        }

        public void SetColor(byte r, byte g, byte b)
        {
            m_graphColor = new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        public void SetBaselineColor(byte r, byte g, byte b)
        {
            m_zeroLineColor = new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        public void SetBackgroundColor(byte r, byte g, byte b)
        {
            canvas1.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        public void ShowBaseline(bool show)
        {
            m_showBaseline = show;
            if (show)
            {
                canvas1.Children.Add(m_line);
            }
            else
            {
                canvas1.Children.Remove(m_line);
            }
        }

        public void HideLegendMinMax()
        {
            canvas1.Margin = new Thickness(0, 0, 0, 0);
            border1.Margin = new Thickness(0, 0, 0, 0);
            labelMin.Visibility = Visibility.Collapsed;
            labelMax.Visibility = Visibility.Collapsed;
        }

        public void SetBaseline(float baseline)
        {
            m_zeroLineValue = baseline;
            MinValue = m_zeroLineValue;
            MaxValue = m_zeroLineValue;
        }
        public void SetBaseline(float baseline, float min, float max)
        {
            m_zeroLineValue = baseline;
            MinValue = min;
            MaxValue = max;
        }

        public void Refresh()
        {
            ConvertToGraphData();
            RenderData();
        }

        private void ConvertToGraphData()
        {
            if (Data.Count < 2)
            {
                return;
            }

            UpdateMinMax();

            m_points = new Point[Data.Count];
            double xSpacing = canvas1.ActualWidth / (Data.Count - 1);
            double ySpacing = canvas1.ActualHeight / (MaxValue - MinValue);
            for (int i = 0; i < Data.Count; ++i)
            {
                double x = xSpacing * i;
                double y = canvas1.ActualHeight - (Data[i] - MinValue) * ySpacing;
                m_points[i] = new Point(x, y);
            }
            m_zeroline[0].X = 0;
            m_zeroline[1].X = canvas1.ActualWidth;
            m_zeroline[0].Y = canvas1.ActualHeight - (m_zeroLineValue - MinValue) * ySpacing;
            m_zeroline[1].Y = m_zeroline[0].Y;

            if (Target.Count >= 2)
            {
                m_targetline = new Point[Target.Count];
                for (int i = 0; i < Target.Count; ++i)
                {
                    double x = xSpacing * i;
                    double v = Target[i];
                    if (v < MinValue)
                        v = MinValue;
                    else if (v > MaxValue)
                        v = MaxValue;
                    
                    double y = canvas1.ActualHeight - (v - MinValue) * ySpacing;
                    m_targetline[i] = new Point(x, y);
                }
            }
            else
            {
                m_targetline = null;
            }

        }

        private void RenderData()
        {
            if (m_points != null)
            {
                m_polyline.Points.Clear();
                for (int i = 0; i < m_points.Length; i++)
                    m_polyline.Points.Add(m_points[i]);

                m_polyline.Stroke = m_graphColor;
            }

            m_targetPolyline.Points.Clear();
            if (m_targetline != null)
            {
                for (int i = 0; i < m_targetline.Length; ++i)
                    m_targetPolyline.Points.Add(m_targetline[i]);
                m_targetPolyline.Stroke = m_targetColor;
            }

            m_line.X1 = m_zeroline[0].X;
            m_line.Y1 = m_zeroline[0].Y;
            m_line.X2 = m_zeroline[1].X;
            m_line.Y2 = m_zeroline[1].Y;
            m_line.Stroke = m_zeroLineColor;
        }

        private void UpdateMinMax()
        {
            for (int i = 0; i < Data.Count; ++i)
            {
                if (Data[i] > MaxValue)
                {
                    MaxValue = Data[i];
                }
                if (Data[i] < MinValue)
                {
                    MinValue = Data[i];
                }
            }

            if (MaxValue - MinValue < 1e-5)
            {
                MaxValue += 1;
                MinValue -= 1;
            }

            if (MaxValue > 1000 || MinValue < -1000)
            {
                if (MaxValue > 1000000 || MinValue < -1000000)
                {
                    labelMin.Content = (MinValue/1000000).ToString("f1") + "M";
                    labelMax.Content = (MaxValue/1000000).ToString("f1") + "M";
                }
                else
                {
                    labelMin.Content = (MinValue/1000).ToString("f1") + "K";
                    labelMax.Content = (MaxValue/1000).ToString("f1") + "K";
                }
            }
            else
            {
                labelMin.Content = MinValue.ToString("f1");
                labelMax.Content = MaxValue.ToString("f1");
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    
    }
}
