using AudioLib;
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
	/// Interaction logic for EnvelopeView.xaml
	/// </summary>
	public partial class EnvelopeView : UserControl
	{
		public EnvelopeView()
		{
			InitializeComponent();

			KnobA.ValueFormatter = FormatValue;
			KnobH.ValueFormatter = FormatValue;
			KnobD.ValueFormatter = FormatValue;
			KnobS.ValueFormatter = FormatValue;
			KnobR.ValueFormatter = FormatValue;
		}

		string FormatValue(double input)
		{
			input = ValueTables.Get(input, ValueTables.Response3Dec) * 5000 - 4.0;
			if(input > 1000)
				return String.Format("{0:0.00}", input * 0.001);
			else if (input > 100)
				return String.Format("{0:0}", input);
			else if(input > 10)
				return String.Format("{0:0.0}", input);
			else
				return String.Format("{0:0.00}", input);

		}

		public void Bind(ModuleBinding module)
		{
			KnobA.SetBinding(module, ParameterBinding.Attack);
			KnobH.SetBinding(module, ParameterBinding.Hold);
			KnobD.SetBinding(module, ParameterBinding.Decay);
			KnobS.SetBinding(module, ParameterBinding.Sustain);
			KnobR.SetBinding(module, ParameterBinding.Release);

			var coll = Extensions.GetLogicalChildCollection<Knob>(this);

			foreach (var knob in coll)
				knob.ValueChanged += ValueChanged;

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

			if (sender != KnobS)
			{
				val = ValueTables.Get(val, ValueTables.Response3Dec) * 5000 - 4.0;
			}

			Binding.UpdateParameter(binding, val);
		}
	}
}
