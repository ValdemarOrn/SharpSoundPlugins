using NetSynth.UI;
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

namespace NetSynth.Tests
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
		}

		void Knob1_ValueChanged(object sender, RoutedEventArgs e)
		{
			var src = sender as Knob;
		}


		private void button1_Click(object sender, RoutedEventArgs e)
		{
			label1.Content = "Bang";
			cb1.IsDropDownOpen = true;
		}

	}
}
