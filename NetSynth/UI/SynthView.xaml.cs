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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetSynth
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SynthView : Window
	{
		private SynthController Pres;

		public SynthView(SynthController synthPresenter)
		{
			this.Pres = synthPresenter;
			InitializeComponent();
			Setup();
		}

		public SynthView()
		{
			InitializeComponent();
			Setup();
		}

		private void Setup()
		{
			if (Pres == null)
				return;


			(Osc1.Panel as WavetableOscView).Bind(ModuleBinding.Osc1);
			(Filter1.Panel as LadderFilterView).Bind(ModuleBinding.Filter1);
			(AmpEnv.Panel as EnvelopeView).Bind(ModuleBinding.AmpEnv);

			Bind();
		}

		private void Bind()
		{
			var coll = Extensions.GetLogicalChildCollection<Knob>(this);

			foreach (var knob in coll)
				knob.ValueChanged += Key_ValueChanged;

			foreach (var knob in coll)
			{
				knob.Value = knob.Value;
				Key_ValueChanged(knob, null);
			}
		}

		void Key_ValueChanged(object sender, RoutedEventArgs e)
		{
			var knob = sender as Knob;
			double val = knob.Value;
			var binding = knob.GetBinding();

			if (Pres != null)
				Pres.SetParameter(binding, val);
		}

		
	}
}
