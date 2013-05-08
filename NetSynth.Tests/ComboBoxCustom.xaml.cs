using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace NetSynth.Tests
{
	/// <summary>
	/// Interaction logic for ComboBoxCustom.xaml
	/// </summary>
	public partial class ComboBoxCustom : UserControl
	{
		public ObservableCollection<ComboBoxItem> ComboBoxItems { get; set; }

		public ComboBoxCustom()
		{
			InitializeComponent();
			this.ComboBoxItems = new ObservableCollection<ComboBoxItem>();
			this.DataContext = this;
		}

		private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
		{
			combobox.IsDropDownOpen = true;
		}

	}
}
