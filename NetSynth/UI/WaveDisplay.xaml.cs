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

namespace NetSynth.UI
{
	/// <summary>
	/// Interaction logic for WaveDisplay.xaml
	/// </summary>
	public partial class WaveDisplay : UserControl
	{
		double[] _data;
		public double[] Data
		{
			get { return _data; }
			set
			{
				_data = value;
				Repaint();
			}
		}

		public WaveDisplay()
		{
			InitializeComponent();
		}

		public void Repaint()
		{
			if (Data == null || Data.Length <= 0)
				return;

			double clearing = 1.0;

			double width = Canvas.ActualWidth - 2 * clearing;
			double height = Canvas.ActualHeight - 2 * clearing;

			if (width == 0 || height == 0)
				return;

			if (Data.Length == 1)
			{
				var p0 = new Point(0, height * 0.5);
				var p1 = new Point(width, height * 0.5);
				Line.Points = new PointCollection(new List<Point>() { p0, p1 });
				return;
			}
			/*
			double min = 1000000000000000;
			double max = -1000000000000000;

			foreach (var val in Data)
			{
				if (val < min)
					min = val;
				if (val > max)
					max = val;
			}

			if ((max - min) < 0.000001)
			{
				max += 0.0000001;
				min -= 0.0000001;
			}*/

			double min = -1;
			double max = 1;

			double dy = max - min;

			var points = new List<Point>();

			for (int i = 0; i < Data.Length; i++)
			{
				double val = Data[i];
				if (Double.IsNaN(val) || Double.IsPositiveInfinity(val) || Double.IsNegativeInfinity(val))
					val = 0.0;

				var x = i / (double)(Data.Length - 1) * width + clearing;
				var y = height - (val - min) / dy * height + clearing;

				points.Add(new Point(x, y));
			}

			Line.Points = new PointCollection(points);
		}

		private void UserControl_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			Repaint();
		}
	}
}
