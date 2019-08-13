using AudioLib.Midi;
using LowProfile.Core.Compilation;
using Microsoft.Win32;
using SharpSoundDevice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PresetSelector
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class ScriptView : UserControl
	{
		public ScriptView()
		{
			InitializeComponent();
			CategoryListBox.SelectionChanged += CategoryListBox_SelectionChanged;
		}

		private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
				CategoryListBox.ScrollIntoView(e.AddedItems[0]);
		}
	}
}
