using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth
{
	public static class ControlBinding
	{
		public static Dictionary<System.Windows.Controls.Control, Binding> ParameterBindings = new Dictionary<System.Windows.Controls.Control, Binding>();

		public static void BindParam(this System.Windows.Controls.Control control, Binding map)
		{
			ParameterBindings[control] = map;
		}

		public static void SetBinding(this System.Windows.Controls.Control control, ModuleBinding module, ParameterBinding parameter)
		{
			var map = new Binding();
			map.Module = module;
			map.Parameter = parameter;

			ParameterBindings[control] = map;
		}

		public static Binding GetBinding(this System.Windows.Controls.Control control)
		{
			if(ParameterBindings.ContainsKey(control))
				return ParameterBindings[control];
			else
				return NetSynth.Binding.None;
		}
	}

	
	public struct Binding
	{
		public ModuleBinding Module;
		public ParameterBinding Parameter;

		public static Binding None;

		static Binding()
		{
			var map = new Binding();
			map.Module = ModuleBinding.None;
			map.Parameter = ParameterBinding.None;
			None = map;
		}

		public static SynthController GlobalController;

		public static void UpdateParameter(Binding binding, object value)
		{
			if(GlobalController != null)
				GlobalController.SetParameter(binding, value);
		}
	}

	public enum ModuleBinding
	{
		None = -1,

		Osc1 = 100,
		Osc2,
		Osc3,
		Osc4,

		Filter1 = 200,
		Filter2,

		AmpEnv = 300,
		ModEnv1,
		ModEnv2,
		ModEnv3,
		ModEnv4,

		LFO1 = 400,
		LFO2,
		LFO3,
		LFO4,

		StepSeq1 = 500,
		StepSeq2,

		FX1 = 600,
		FX2,
		FX3,
		FX4,

		InsFx1 = 700,
		InsFx2
	}

	public enum ParameterBinding
	{
		None = -1,

		// Oscillator Parameters
		Wave = 100,
		Octave,
		Semi,
		Cent,
		PhaseStart,
		PhaseFree,
		WavePosition,

		// Filter Parameters
		Cutoff = 200,
		Resonance,
		X,
		A,
		B,
		C,
		D,

		// Envelopes
		Attack = 300,
		Hold,
		Decay,
		Sustain,
		Release,

		// LFO
		Freq = 400,
		Phase
	}
}
