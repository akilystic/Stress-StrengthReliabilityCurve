using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PropertyChanged;

namespace ReliabilityCurve
{
    [AddINotifyPropertyChangedInterface]
    public partial class MainWindow : Window
    {
        public PlotModel PlotModel { get; set; }


        public decimal StressMu { get; set; }
        public decimal StrengthMu { set; get; }

        public double StressOverlapArea { get; set; }
        public double StrengthOverlapArea { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void PlotStressAndStrength()
        {
            PlotModel = new PlotModel { Title = "Probability Density" };

            // Sample data
            var stressDistribution = new MathNet.Numerics.Distributions.Normal((double)StressMu, 7);
            var strengthDistribution = new MathNet.Numerics.Distributions.Normal((double)StrengthMu, 7);

            var stressSeries = CreateDensitySeries(stressDistribution, "Stress");
            var strengthSeries = CreateDensitySeries(strengthDistribution, "Strength");

            PlotModel.Series.Add(stressSeries.Item1);
            PlotModel.Series.Add(strengthSeries.Item1);


            PlotModel.Series.Add(stressSeries.Item2);
            PlotModel.Series.Add(strengthSeries.Item2);

            // Customize axes

            // Create the x-axis with grid lines
            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid, // Enable major grid lines
                MinorGridlineStyle = LineStyle.Dot,   // Enable minor grid lines
                Title = "Units"
            };

            // Create the y-axis with grid lines
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid, // Enable major grid lines
                MinorGridlineStyle = LineStyle.Dot,   // Enable minor grid lines
                Title = "Probability Density"
            };

            PlotModel.Axes.Add(xAxis);
            PlotModel.Axes.Add(yAxis);

            plotView?.InvalidatePlot(true);


            int numSegments = 1000; // Adjust the number of segments for desired accuracy

            double xMin = Math.Max(stressDistribution.Mean - (4 * stressDistribution.StdDev), strengthDistribution.Mean - (4 * strengthDistribution.StdDev));
            double xMax = Math.Min(stressDistribution.Mean + (4 * stressDistribution.StdDev), strengthDistribution.Mean + (4 * strengthDistribution.StdDev));
            double dx = (xMax - xMin) / numSegments;

            double sum = 0;

            for (int i = 0; i <= numSegments; i++)
            {
                double x = xMin + (i * dx);
                double pdf1 = stressDistribution.Density(x);
                double cdf2 = strengthDistribution.CumulativeDistribution(x);
                double value = pdf1 * (1 - cdf2); // Survival Function = 1 - CDF
                if (i == 0 || i == numSegments)
                {
                    sum += value / 2;
                }
                else
                {
                    sum += value;
                }
            }

            double failureProbability = sum * dx;
            StressOverlapArea = failureProbability;
        }

        private Tuple<LineSeries, AreaSeries> CreateDensitySeries(MathNet.Numerics.Distributions.Normal distribution, string title)
        {
            var lineSeries = new LineSeries
            {
                Title = title,
                //Color = lineColor,
                MarkerType = MarkerType.None
            };

            var areaSeries = new AreaSeries
            {
                // Color = fillColor,
                StrokeThickness = 1
            };

            double xMin = distribution.Mean - (4 * distribution.StdDev);
            double xMax = distribution.Mean + (4 * distribution.StdDev);
            double step = (xMax - xMin) / 100.00;

            for (double x = xMin; x <= xMax; x += step)
            {
                double y = distribution.Density(x);
                lineSeries.Points.Add(new DataPoint(x, y));
                areaSeries.Points.Add(new DataPoint(x, y));
            }
            return Tuple.Create(lineSeries, areaSeries);
        }
        private void stressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var s = sender as System.Windows.Controls.Slider;

            if (s != null)
            {
                StressMu = (int)s.Value;
                PlotStressAndStrength();
            }
        }
        private void strengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var s = sender as System.Windows.Controls.Slider;

            if (s != null)
            {
                StrengthMu = (int)s.Value;
                PlotStressAndStrength();
            }
        }
    }
}



