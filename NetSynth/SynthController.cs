using AudioLib;
using NetSynth.Modules;
using NetSynth.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth
{
	public class SynthController
	{
		public AudioDeviceModule AudioDevice;
		public SynthView View;
		public VoiceManager Manager;

		/// <summary>
		/// Stores additional data that is needed for some modules
		/// </summary>
		public Dictionary<ModuleBinding, string> SerializedData;

		public SynthController(AudioDeviceModule audioDevice)
		{
			SerializedData = new Dictionary<ModuleBinding, string>();
			Binding.GlobalController = this;
			AudioDevice = audioDevice;
			Manager = new VoiceManager(1);
		}

		public void SetSamplerate(double value)
		{
			foreach (var voice in Manager.Voices)
			{
				voice.Osc1.Samplerate = value;
				voice.Osc2.Samplerate = value;
				voice.Filter1.Samplerate = value;
				voice.Filter2.Samplerate = value;
				voice.AmpEnv.Samplerate = value;
			}
		}

		public void SetWave(double[] wave, ModuleBinding module, int waveIndex)
		{
			foreach (var voice in Manager.Voices)
			{
				IOscillator osc = null;

				if (module == ModuleBinding.Osc1)
					osc = voice.Osc1;
				else if (module == ModuleBinding.Osc2)
					osc = voice.Osc2;
				//else if (module == ModuleBinding.Osc3)
				//	osc = voice.Osc3;
				//else if (module == ModuleBinding.Osc4)
				//	osc = voice.Osc4;

				if (osc == null || !(osc is WavetableOsc))
					continue;

				WavetableOsc wo = osc as WavetableOsc;
				wo.SetWave(wave, waveIndex);
			}
		}

		public void NoteOn(int note, double velocity)
		{
			Manager.NoteOn(note, velocity);
		}

		public void NoteOff(int note)
		{
			Manager.NoteOff(note);
		}

		public void SetPitchWheel(double value)
		{
			for (int i = 0; i < Manager.Voices.Length; i++)
				Manager.Voices[i].MidiInput.PitchBend = value;
		}

		internal void CreateView()
		{
			View = new SynthView(this);
		}

		public void SetParameter(ModuleBinding module, ParameterBinding param, object value)
		{
			var binding = new Binding();
			binding.Module = module;
			binding.Parameter = param;
			SetParameter(binding, value); 
		}

		public void SetParameter(Binding binding, object value)
		{
			switch(binding.Module)
			{
				case ModuleBinding.Filter1:
					SetParamFilter(binding, value);
					return;
				case ModuleBinding.Filter2:
					SetParamFilter(binding, value);
					return;
				case ModuleBinding.Osc1:
					SetParamOsc(binding, value);
					return;
				case ModuleBinding.Osc2:
					SetParamOsc(binding, value);
					return;
				case ModuleBinding.Osc3:
					SetParamOsc(binding, value);
					return;
				case ModuleBinding.AmpEnv:
					SetParamEnv(binding, value);
					return;
				case ModuleBinding.ModEnv1:
					SetParamEnv(binding, value);
					return;
				case ModuleBinding.ModEnv2:
					SetParamEnv(binding, value);
					return;
				case ModuleBinding.ModEnv3:
					SetParamEnv(binding, value);
					return;
				case ModuleBinding.ModEnv4:
					SetParamEnv(binding, value);
					return;
			}
		}

		

		private void SetParamOsc(Binding binding, object value)
		{
			if (binding.Parameter == ParameterBinding.Wave)
			{
				//SetWave((double)value);
				return;
			}

			IOscillator module = null;

			foreach (var voice in Manager.Voices)
			{
				if (binding.Module == ModuleBinding.Osc1)
					module = voice.Osc1;
				else if (binding.Module == ModuleBinding.Osc2)
					module = voice.Osc2;
				else
					return;

				switch (binding.Parameter)
				{
					case ParameterBinding.Octave:
						module.Octave = (double)value; return;
					case ParameterBinding.Semi:
						module.Semi = (double)value; return;
					case ParameterBinding.Cent:
						module.Cent = (double)value; return;
					case ParameterBinding.WavePosition:
						(module as WavetableOsc).CurrentWaveIndex = (double)value; return;
				}
			}
		}

		private void SetParamEnv(Binding binding, object value)
		{
			AudioLib.Modules.Ahdsr module = null;

			foreach (var voice in Manager.Voices)
			{
				if (binding.Module == ModuleBinding.AmpEnv)
					module = voice.AmpEnv;
				else if (binding.Module == ModuleBinding.ModEnv1)
					module = voice.ModEnv1;
				else if (binding.Module == ModuleBinding.ModEnv2)
					module = voice.ModEnv2;
				else if (binding.Module == ModuleBinding.ModEnv3)
					module = voice.ModEnv3;
				else if (binding.Module == ModuleBinding.ModEnv4)
					module = voice.ModEnv4;
				else
					return;

				switch (binding.Parameter)
				{
					case (ParameterBinding.Attack):
						module.Attack = (double)value; return;
					case (ParameterBinding.Hold):
						module.Hold = (double)value; return;
					case (ParameterBinding.Decay):
						module.Decay = (double)value; return;
					case (ParameterBinding.Sustain):
						module.Sustain = (double)value; return;
					case (ParameterBinding.Release):
						module.Release = (double)value; return;
				}
			}
		}

		private void SetParamFilter(Binding binding, object value)
		{
			NetSynth.Modules.LadderFilter module = null;

			foreach(var voice in Manager.Voices)
			{
				if (binding.Module == ModuleBinding.Filter1)
					module = voice.Filter1;
				else if (binding.Module == ModuleBinding.Filter2)
					module = voice.Filter2;
				else
					return;

				switch(binding.Parameter)
				{
					case (ParameterBinding.Cutoff):
						module.Cutoff = (double)value; return;
					case (ParameterBinding.Resonance):
						module.Resonance = (double)value; return;
					case (ParameterBinding.X):
						module.VX = (double)value; return;
					case (ParameterBinding.A):
						module.VA = (double)value; return;
					case (ParameterBinding.B):
						module.VB = (double)value; return;
					case (ParameterBinding.C):
						module.VC = (double)value; return;
					case (ParameterBinding.D):
						module.VD = (double)value; return;
				}
			}
		}
	}
}
