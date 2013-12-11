using NetSynth.WaveDesigner;
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
	/// Interaction logic for Oscillator.xaml
	/// </summary>
	public partial class WavetableOscView : UserControl
	{
		public WavetableOscView()
		{
			InitializeComponent();
			
		}

		ModuleBinding Module;

		public void Bind(ModuleBinding module)
		{
			Module = module;
			KnobOctave.SetBinding(module, ParameterBinding.Octave);
			KnobSemi.SetBinding(module, ParameterBinding.Semi);
			KnobCent.SetBinding(module, ParameterBinding.Cent);
			KnobWavePosition.SetBinding(module, ParameterBinding.WavePosition);

			var coll = Extensions.GetLogicalChildCollection<Knob>(this);

//			foreach (var knob in coll)
//				knob.ValueChanged += ValueChanged;

			foreach (var knob in coll)
			{
				knob.Value = knob.Value;
				ValueChanged(knob, null);
			}
		}

		private void ValueChanged(object sender, RoutedEventArgs e)
		{
			var knob = sender as Knob;
			double val = knob.Value;
			var binding = knob.GetBinding();

			Binding.UpdateParameter(binding, val);
		}

		private void SetWaveClick(object sender, RoutedEventArgs e)
		{
			WaveGen.WaveGenWindow view = new WaveGen.WaveGenWindow();
			view.Wavegen = new WaveGen.SawtoothGen();
			view.Module = this.Module;

			view.Wavegen.SampleCount = 2048;
			view.Wavegen.WaveCount = 32;
			view.Refresh();
			view.Show();
		}
	}
}
