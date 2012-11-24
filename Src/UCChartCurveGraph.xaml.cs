using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections;

namespace SimpleWPFChart
{
    /// <summary>
    /// Interaction logic for UCChartCurveGraph.xaml
    /// </summary>
    public partial class UCChartCurveGraph : Grid
    {
        private enum Levels
        {
            Low,
            Medium,
            High
        }

        #region Variables
        private ObservableCollection<ChartRequestInfo> ocRawData;
        private ObservableCollection<ChartCurve> ocGraphData;
        
        private SolidColorBrush bgColorChartArea;
        private SolidColorBrush ucBackground = Brushes.LightGreen;

        private int supportedDataPointsCount = 20;
        private double minYValue = 0;       
        private double maxYValue = 100;
        private double minXValue = 0;
        //private double maxXValue = 0;        

        #endregion

        #region Properties
        public ObservableCollection<ChartRequestInfo> RequestData
        {
            set
            {
                ocRawData = value;
            }
        }

        public new SolidColorBrush Background
        {
            get { return this.Background; }
        }

        public SolidColorBrush ChartBgColor
        {
            get;
            set;
        }

        public SolidColorBrush FrameBgColor
        {
            get { return bgColorChartArea; }
            set { bgColorChartArea = value; }
        }

        public string GraphTitle { get; set; }
        public bool ShowTitle { get; set; }
        public bool ShowLegendTable { get; set; } 
       
        public bool ShowXYLabel { get; set; }
        public string XAxisLabelString { get; set; }
        public string YAxisLabelString { get; set; }

        public bool ShowYTicks { get; set; }
        public bool ShowXTicks { get; set; }
        public bool DataClipping { get; set; }

        #endregion

        public UCChartCurveGraph()
        {
            InitializeComponent();            
        }

        public UCChartCurveGraph(ObservableCollection<ChartRequestInfo> requestData)
        {
            InitializeComponent();
            ocRawData = requestData;            
        }

        /// <summary>
        /// Convert raw data of lines to the graph coordinates.
        /// </summary>
        /// <param name="rawlines"></param>
        private void ConvertToGraphData()
        {
            if (ocRawData == null)
            {
                return;
            }
            
            ocGraphData = new ObservableCollection<ChartCurve>();
            
            foreach (ChartRequestInfo request in ocRawData)
            {
                ocGraphData.Add(new ChartCurve(request.Color, new Point[request.Data.Count], request.ChartLineType, request.Legend));
                double xSpacing = borderChart.ActualWidth / (request.Data.Count - 1);
                double ySpacing = borderChart.ActualHeight / (maxYValue - minYValue);


                int index = ocRawData.IndexOf(request);
                for (int i = 0; i < request.Data.Count; i++)
                {
                    double value = request.Data[i];

                    if (DataClipping)
                    {
                        if (value > maxYValue)
                            value = maxYValue;
                        if (value < minYValue)
                            value = minYValue;
                    }

                    Point convertedPoint = new Point();
                    convertedPoint.Y = (borderChart.ActualHeight - value * ySpacing);
                    convertedPoint.X = xSpacing * i;
                    
                    ocGraphData[index].Points[i] = convertedPoint;
                }
            }            
        }

        private void DrawPolylineChartData(SolidColorBrush color, Point []points)
        {
            //This code uses simple Polyline obj instead of PolyLineSegment method
            Polyline polyline = new Polyline();
            for (int i = 0; i < points.Length; i++)
                polyline.Points.Add(points[i]);

            polyline.Stroke = color;
            borderChart.Children.Add(polyline);
            

            ////This code uses PolyLineSegment but somehow it doesn't handle IsMouseOver correctly. 
            //PathSegmentCollection psc = new PathSegmentCollection();
            
            //for (int i = 0; i < 20-1; ++i)
            //{
            //    List<Point> arraypoint = new List<Point>();
            //    arraypoint.Add(points[i]);
            //    arraypoint.Add(points[i+1]);
            //    PolyLineSegment pls = new PolyLineSegment(arraypoint, true);
            //    psc.Add(pls);
            //}

            //PathFigure tryf = new PathFigure(points[0], psc, false);
            //PathGeometry tryg = new PathGeometry(new PathFigure[] { tryf });
            //Path trypath = new Path() { Stroke = color , StrokeThickness = 1, Data = tryg };
            //trypath.MouseEnter += new MouseEventHandler(trypath_MouseEnter);  //must do this to handle Mouse events properly
            //trypath.MouseLeave += new MouseEventHandler(trypath_MouseLeave);  //must do this to handle Mouse events properly
            //this.borderChart.Children.Add(trypath);
        }

        private void trypath_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Path).StrokeThickness = 2;
        }

        private void trypath_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Path).StrokeThickness = 3;
        }

        private void DrawBezierChartData(SolidColorBrush color, Point[] points, Point[] firstControlPoints, Point[] secondControlPoints)
        {
            PathSegmentCollection lines = new PathSegmentCollection();
            for (int i = 0; i < points.Length - 1; ++i)
            {
                lines.Add(new BezierSegment(firstControlPoints[i], secondControlPoints[i], points[i + 1], true));
            }

            PathFigure pf = new PathFigure(points[0], lines, false);
            
            PathGeometry pg = new PathGeometry(new PathFigure[] { pf });
            Path linepath = new Path() { Stroke = color, Data = pg };
            linepath.Style = this.Resources["pathStyleKey"] as Style;            
            this.borderChart.Children.Add(linepath);


            //// Print points
            //Trace.WriteLine(string.Format("Start=({0})", points[0]));
            //for (int i = 0; i < points.Length - 1; ++i)
            //{
            //    Trace.WriteLine(string.Format("FIRST=({0}) SECOND=({1}) STOP=({2})"
            //        , first[i], second[i], points[i + 1]));
            //}
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Step 1: Update UI with properties
            labelTitle.Visibility = ShowTitle ? Visibility.Visible : Visibility.Collapsed;
            labelTitle.Content = GraphTitle;

            textblockXAxis.Visibility = ShowXYLabel ? Visibility.Visible : Visibility.Collapsed;
            textblockYAxis.Visibility = ShowXYLabel ? Visibility.Visible : Visibility.Collapsed;
            textblockXAxis.Text = XAxisLabelString;
            textblockYAxis.Text = YAxisLabelString;
            dockpanelXTicks.Visibility = ShowXTicks ? Visibility.Visible : Visibility.Collapsed;
            dockpanelYTicks.Visibility = ShowYTicks ? Visibility.Visible : Visibility.Collapsed;
            

            borderFrame.Background = FrameBgColor;
            //borderUC.Background = ChartBgColor;
            stackpanelLegendTable.Visibility = ShowLegendTable ? Visibility.Visible : Visibility.Collapsed;

            //Step 2: Convert given data into graph data
            ConvertToGraphData();

            if (ocGraphData == null) return;

            //Step 3: Render data
            foreach (ChartCurve cc in ocGraphData)
            {
                Point[] knots = new Point[cc.Points.Length];
                cc.Points.CopyTo(knots, 0);

                if ((cc.ChartLineType == ChartLineType.BezierType) || (cc.ChartLineType == ChartLineType.BezierKnotsType))
                {
                    Point[] first;
                    Point[] second;

                    ovp.BezierSpline.GetCurveControlPoints(knots, out first, out second);

                    //Step 4: Clip
                    if (DataClipping)
                    {
                        for (int i = 0; i < knots.Length; i++)
                        {
                            if (knots[i].Y < minYValue) knots[i].Y = minYValue;
                            if (knots[i].Y > borderChart.ActualHeight) knots[i].Y = borderChart.ActualHeight;
                        }
                        for (int j = 0; j < first.Length; j++)
                        {
                            if (first[j].Y < minYValue) first[j].Y = minYValue;
                            if (first[j].Y > borderChart.ActualHeight) first[j].Y = borderChart.ActualHeight;
                            if (second[j].Y < minYValue) second[j].Y = minYValue;
                            if (second[j].Y > borderChart.ActualHeight) second[j].Y = borderChart.ActualHeight;
                        }        
                    }

                    DrawBezierChartData(cc.Color, knots, first, second);

                    if (cc.ChartLineType == ChartLineType.BezierKnotsType)
                    {
                        const double markerSize = 5;
                        // Step 5: Draw Points on Curve
                        for (int i = 0; i < knots.Length; ++i)
                        {
                            Rectangle rect = new Rectangle()
                            {
                                Stroke = cc.Color,
                                Fill = cc.Color,
                                Height = markerSize,
                                Width = markerSize
                            };
                            Canvas.SetLeft(rect, knots[i].X - markerSize / 2);
                            Canvas.SetTop(rect, knots[i].Y - markerSize / 2);
                            borderChart.Children.Add(rect);
                        }
                    }
                }
                else
                {
                    //Step 4: Clip
                    if (DataClipping)
                    {
                        for (int i = 0; i < knots.Length; i++)
                        {
                            if (knots[i].Y < minYValue) knots[i].Y = minYValue;
                            if (knots[i].Y > borderChart.ActualHeight) knots[i].Y = borderChart.ActualHeight;
                        }
                    }

                    DrawPolylineChartData(cc.Color, knots);

                    if (cc.ChartLineType == ChartLineType.PolylineKnotsType)
                    {
                        const double markerSize = 5;
                        // Step 5: Draw Points on Curve
                        for (int i = 0; i < knots.Length; ++i)
                        {
                            Rectangle rect = new Rectangle()
                            {
                                Stroke = cc.Color,
                                Fill = cc.Color,
                                Height = markerSize,
                                Width = markerSize
                            };
                            Canvas.SetLeft(rect, knots[i].X - markerSize / 2);
                            Canvas.SetTop(rect, knots[i].Y - markerSize / 2);
                            borderChart.Children.Add(rect);
                        }
                    }
                }
            }

            if (ShowLegendTable)
                DrawLegends();

            if (ShowXTicks)
            {
                labelMinXValue.Content = this.minXValue.ToString();
                labelMaxXValue.Content = this.supportedDataPointsCount.ToString();
            }

            if (ShowYTicks)
            {
                labelMinYValue.Content = this.minYValue.ToString();
                labelMaxYValue.Content = this.maxYValue.ToString();
            }
        }

        private void DrawLegends()
        {
            foreach (ChartCurve cc in ocGraphData)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Border b = new Border();
                b.Width = 12;
                b.Height = 8;
                b.BorderBrush = Brushes.Black;
                b.Background = cc.Color;
                b.Margin = new Thickness(3,0,4,0);
                sp.Children.Add(b);
                TextBlock tb = new TextBlock();
                tb.Text = cc.Legend;
                sp.Children.Add(tb);
                stackpanelLegend.Children.Add(sp);
            }
        }        
    }

    public enum ChartLineType
    {
        PolylineType,
        BezierType,
        PolylineKnotsType,
        BezierKnotsType,
    }

    public class ChartRequestInfo
    {
        private SolidColorBrush color;
        private ObservableCollection<double> data;
        private ChartLineType chartLineType;
        private string legend;

        public ChartRequestInfo(SolidColorBrush color, ObservableCollection<double> data, ChartLineType clt, string legendString)
        {
            this.color = color;
            this.data = data;
            this.chartLineType = clt;
            this.legend = legendString;
        }

        public SolidColorBrush Color
        {
            get { return color; }
        }

        public ObservableCollection<double> Data
        {
            get { return data; }
        }

        public ChartLineType ChartLineType
        {
            get { return chartLineType; }
        }

        public string Legend
        {
            get { return this.legend; }
        }
    }

    public class ChartCurve
    {
        public ChartCurve(SolidColorBrush color, Point[] points, ChartLineType type, string legendString)
        {
            this.color = color;
            this.points = points;
            this.clt = type;
            this.legend = legendString;
        }
        public SolidColorBrush Color
        {
            get { return this.color; }
            set { this.color = value; }
        }
        public Point[] Points
        {
            get { return this.points; }
            set { this.points = value; }
        }
        public ChartLineType ChartLineType
        {
            get { return this.clt; }
            set { this.clt = value; }
        }

        public string Legend
        {
            get { return this.legend; }
            set { this.legend = value; }
        }

        private SolidColorBrush color;
        private Point[] points;
        private ChartLineType clt;
        private string legend;
    }
}