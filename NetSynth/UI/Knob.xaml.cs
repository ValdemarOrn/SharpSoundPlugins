using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	/// Interaction logic for Knob.xaml
	/// </summary>
	public partial class Knob : UserControl
	{
		static internal DependencyProperty ValueProperty;
		static internal DependencyProperty DeltaProperty;
		static internal DependencyProperty ShowValueProperty;
		static internal DependencyProperty StrokeAProperty;
		static internal DependencyProperty StrokeBProperty;
		static internal DependencyProperty CaptionProperty;
		static internal DependencyProperty ShowCenterProperty;

		public static readonly RoutedEvent ValueChangeEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Knob));

		public event RoutedEventHandler ValueChanged
		{
			add { AddHandler(ValueChangeEvent, value); }
			remove { RemoveHandler(ValueChangeEvent, value); }
		}

		static Knob()
		{
			ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(Knob));
			DeltaProperty = DependencyProperty.Register("Delta", typeof(double), typeof(Knob));
			ShowValueProperty = DependencyProperty.Register("ShowValue", typeof(bool), typeof(Knob));
			StrokeAProperty = DependencyProperty.Register("StrokeA", typeof(Brush), typeof(Knob));
			StrokeBProperty = DependencyProperty.Register("StrokeB", typeof(Brush), typeof(Knob));
			CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(Knob));
			ShowCenterProperty = DependencyProperty.Register("ShowCenter", typeof(Visibility), typeof(Knob));
		}

		public Knob()
		{
			InitializeComponent();

			// add change listeners to props
			DependencyPropertyDescriptor prop;
			prop = DependencyPropertyDescriptor.FromProperty(ValueProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => Paint());
			prop = DependencyPropertyDescriptor.FromProperty(DeltaProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => Paint());
			prop = DependencyPropertyDescriptor.FromProperty(ShowValueProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => Paint());
			prop = DependencyPropertyDescriptor.FromProperty(StrokeAProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => Paint());
			prop = DependencyPropertyDescriptor.FromProperty(StrokeBProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => Paint());
			prop = DependencyPropertyDescriptor.FromProperty(CaptionProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => Paint());
			prop = DependencyPropertyDescriptor.FromProperty(ShowCenterProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => Paint());

			ShowCenter = System.Windows.Visibility.Hidden;
			Min = 0;
			Max = 1;
			Steps = 1000;
			Value = 0;
			Delta = 0.005;
			ShowValue = false;
			StrokeA = new SolidColorBrush(Colors.CornflowerBlue);
			StrokeB = new SolidColorBrush(Colors.Black);
			Centerer.Fill = this.StrokeB;

			Paint();
		}


		public Func<double, string> ValueFormatter { get; set; }

		public double Min { get; set; }
		public double Max { get; set; }
		public int Steps { get; set; }

		double _internalVal;

		public double Value
		{
			get { return (double)base.GetValue(ValueProperty); }
			set
			{
				// warp to internal, then back to Value
				// give the correct step
				double val = CalculateInternalValue(value);
				_internalVal = val;
				val = CalculateValue(val);

				SetValue(ValueProperty, val);
				
				var args = new RoutedEventArgs(ValueChangeEvent);
				RaiseEvent(args);
				Paint();
			}
		}

		public double Delta
		{
			get { return (double)base.GetValue(DeltaProperty); }
			set { SetValue(DeltaProperty, value); }
		}

		public bool ShowValue
		{
			get { return (bool)base.GetValue(ShowValueProperty); }
			set { SetValue(ShowValueProperty, value); }
		}

		public Brush StrokeA
		{
			get { return (Brush)base.GetValue(StrokeAProperty); }
			set { SetValue(StrokeAProperty, value); }
		}

		public Brush StrokeB
		{
			get { return (Brush)base.GetValue(StrokeBProperty); }
			set { SetValue(StrokeBProperty, value); }
		}

		public string Caption
		{
			get { return (string)base.GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}

		public Visibility ShowCenter
		{
			get { return (Visibility)base.GetValue(ShowCenterProperty); }
			set { SetValue(ShowCenterProperty, value); }
		}

		bool Selected;
		Point MousePos;

		private void UserControl_MouseDown_1(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Released)
				return;

			Selected = true;
			_internalVal = CalculateInternalValue(Value);
			Mouse.Capture(this);
			MousePos = e.GetPosition(this);
		}

		private void UserControl_MouseUp_1(object sender, MouseButtonEventArgs e)
		{
			Selected = false;
			Mouse.Capture(null);
			MousePos = e.GetPosition(this);
			_internalVal = CalculateInternalValue(Value);
			Paint();
		}

		private void UserControl_MouseMove_1(object sender, MouseEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Released)
			{
				Selected = false;
				Mouse.Capture(null);
				MousePos = e.GetPosition(this);
				_internalVal = CalculateInternalValue(Value);
				return;
			}

			var oldPos = MousePos;
			MousePos = e.GetPosition(this);

			if (!Selected)
				return;

			var dx = oldPos.Y - MousePos.Y;

			if (Math.Abs(dx) < 0.5)
				return;

			var oldVal = _internalVal;
			var val = _internalVal + Delta * dx;

			if (val < 0.0)
				val = 0.0;
			else if (val > 1.0)
				val = 1.0;

			if (val != oldVal)
			{
				_internalVal = val;
				double vv = CalculateValue(val);
				if (vv != Value)
					Value = vv;

				
			}
		}

		private double CalculateValue(double internalValue)
		{
			int steps = Steps;
			if (steps <= 1)
				steps = 10000;

			internalValue = internalValue + 1.0 / (steps-1) * 0.5;
 
			int position = (int)(internalValue * (steps - 1) + 0.000001);
			double val = position / (double)(steps-1);
			double span = (Max - Min);
			val = val * span + Min;
			return val;
		}

		private double CalculateInternalValue(double value)
		{
			double span = (Max - Min);
			double val = (value - Min) / span;
			return val;
		}

		private void Paint()
		{
			if (ShowValue)
				LabelValue.Visibility = System.Windows.Visibility.Visible;
			else
				LabelValue.Visibility = System.Windows.Visibility.Hidden;

			if (Caption == null || Caption == "")
				LabelCaption.Content = " ";
			else
				LabelCaption.Content = Caption;

			if(ValueFormatter != null)
				LabelValue.Content = ValueFormatter(Value);
			else
				LabelValue.Content = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00}", Value);

			PathA.Stroke = StrokeA;
			PathB.Stroke = StrokeB;

			double radius = 50.0;
			double mid = 50.0;

			ArcLeft.Size = new Size(radius, radius);
			ArcRight.Size = new Size(radius, radius);

			double startAngle = 225 / 360.0 * 2 * Math.PI;
			double endAngle = -45 / 360.0 * 2 * Math.PI;

			double valueDegrees = CalculateInternalValue(Value) * 270.0;

			// prevents either part of the arc from disappearing completely
			// if it does then the viewbox causes a slight jump on the screen
			if (valueDegrees < 0.1)
				valueDegrees = 0.1;
			else if (valueDegrees > 269.9)
				valueDegrees = 269.9;

			double valueAngle =  (225 - valueDegrees) / 360.0 * 2 * Math.PI;

			double startX = Math.Cos(startAngle) * radius + mid;
			double endX = Math.Cos(endAngle) * radius + mid;
			double startEndY = -Math.Sin(startAngle) * radius + mid;

			double valueX = Math.Cos(valueAngle) * radius + mid;
			double valueY = -Math.Sin(valueAngle) * radius + mid;

			PathLeft.StartPoint = new Point(startX, startEndY);
			PathRight.StartPoint = new Point(endX, startEndY);

			ArcLeft.Point = new Point(valueX, valueY);
			ArcRight.Point = new Point(valueX, valueY);

			

			if(valueDegrees < 180)
				ArcLeft.IsLargeArc = false;
			else
				ArcLeft.IsLargeArc = true;

			if (270.0 - valueDegrees < 180)
				ArcRight.IsLargeArc = false;
			else
				ArcRight.IsLargeArc = true;
			
		}

		private void Centerer_MouseEnter(object sender, MouseEventArgs e)
		{
			Centerer.Fill = this.StrokeA;
		}

		private void Centerer_MouseLeave(object sender, MouseEventArgs e)
		{
			Centerer.Fill = this.StrokeB;
		}

		private void Centerer_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Value = CalculateValue(0.5);
			var args = new RoutedEventArgs(ValueChangeEvent);
			RaiseEvent(args);
		}
	}
}
