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
	/// Interaction logic for LadderFilter.xaml
	/// </summary>
	public partial class LadderFilterView : UserControl
	{
		public LadderFilterView()
		{
			InitializeComponent();
		}

		public void Bind(ModuleBinding module)
		{
			KnobCutoff.SetBinding(module, ParameterBinding.Cutoff);
			KnobResonance.SetBinding(module, ParameterBinding.Resonance);
			KnobX.SetBinding(module, ParameterBinding.X);
			KnobA.SetBinding(module, ParameterBinding.A);
			KnobB.SetBinding(module, ParameterBinding.B);
			KnobC.SetBinding(module, ParameterBinding.C);
			KnobD.SetBinding(module, ParameterBinding.D);

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
	}
}
