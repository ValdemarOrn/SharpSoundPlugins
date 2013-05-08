using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace NetSynth.WaveDesigner
{
	/// <summary>
	/// Interaction logic for ParameterEditor.xaml
	/// </summary>
	public partial class ParameterEditor : Window
	{
		DynamicBlockParameter _param;
		public DynamicBlockParameter Parameter
		{
			get { return _param;}
			set { _param = value; ShowData(); }
		}

		public ParameterEditor()
		{
			InitializeComponent();
		}

		public void ShowData()
		{
			TextBoxName.Text = Parameter.Name;

			if (!Parameter.IsInteger)
			{
				TextBoxValue.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.000}", Parameter.ValueStart);
				TextBoxMin.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.000}", Parameter.Min);
				TextBoxMax.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.000}", Parameter.Max);
			}
			else
			{
				TextBoxValue.Text = String.Format(CultureInfo.InvariantCulture, "{0:0}", Parameter.ValueStart);
				TextBoxMin.Text = String.Format(CultureInfo.InvariantCulture, "{0:0}", Parameter.Min);
				TextBoxMax.Text = String.Format(CultureInfo.InvariantCulture, "{0:0}", Parameter.Max);
			}

			CheckBoxIsInteger.IsChecked = Parameter.IsInteger;
		}

		private void SaveClick(object sender, RoutedEventArgs e)
		{
			TextBoxMin.Foreground = Brushes.Black;
			TextBoxMax.Foreground = Brushes.Black;
			TextBoxValue.Foreground = Brushes.Black;

			bool parsed = false;

			// --------------------- Parse Min ---------------------
			double min = 0.0;
			parsed = Double.TryParse(TextBoxMin.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out min);
			if(!parsed)
			{
				TextBoxMin.Foreground = Brushes.Red;
				return;
			}

			// --------------------- Parse Max ---------------------
			double max = 0.0;
			parsed = Double.TryParse(TextBoxMax.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out max);
			if (!parsed)
			{
				TextBoxMax.Foreground = Brushes.Red;
				return;
			}

			// min must be greater
			if(max <= min)
			{
				TextBoxMin.Foreground = Brushes.Red;
				return;
			}

			// --------------------- Parse IsInteger ---------------------

			bool isInt = CheckBoxIsInteger.IsChecked.GetValueOrDefault();
			if(isInt)
			{
				min = Math.Floor(min);
				max = Math.Floor(max);
				if(max - min < 1)
				{
					TextBoxMin.Foreground = Brushes.Red;
					return;
				}
			}

			// --------------------- Parse Value ---------------------

			double value = 0.0;
			parsed = Double.TryParse(TextBoxValue.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
			if (!parsed || value < min || value > max)
			{
				TextBoxValue.Foreground = Brushes.Red;
				return;
			}

			// --------------------- Assign to model ---------------------

			Parameter.Name = TextBoxName.Text;
			Parameter.ValueStart = value;
			Parameter.ValueStop = value;
			Parameter.Min = min;
			Parameter.Max = max;
			Parameter.IsInteger = isInt;

			this.Close();
		}
	}
}
