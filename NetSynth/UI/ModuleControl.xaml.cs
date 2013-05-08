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
	/// Interaction logic for ModuleControl.xaml
	/// </summary>
	public partial class ModuleControl : UserControl
	{
		static internal DependencyProperty PanelProperty;
		static internal DependencyProperty TitleProperty;

		static ModuleControl()
		{
			PanelProperty = DependencyProperty.Register("Panel", typeof(Control), typeof(ModuleControl));
			TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModuleControl));
		}

		public Control Panel
		{
			get { return (Control)base.GetValue(PanelProperty); }
			set { SetValue(PanelProperty, value); }
		}

		public string Title
		{
			get { return (string)base.GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}


		public ModuleControl()
		{
			InitializeComponent();

			DependencyPropertyDescriptor prop;
			prop = DependencyPropertyDescriptor.FromProperty(PanelProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => this.InvalidateVisual());
			prop = DependencyPropertyDescriptor.FromProperty(TitleProperty, this.GetType());
			prop.AddValueChanged(this, (sender, args) => this.InvalidateVisual());
		}

	}
}
