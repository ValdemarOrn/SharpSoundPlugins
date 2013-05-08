using AudioLib;
using AudioLib.Modules;
using NetSynth.Modulation;
using NetSynth.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth
{
	public sealed class Voice
	{
		// ---------------------------- Synth Parts ----------------------------

		public IOscillator Osc1, Osc2;
		public LadderFilter Filter1, Filter2;
		public Ahdsr AmpEnv, Filter1Env, Filter2Env, ModEnv1, ModEnv2, ModEnv3, ModEnv4;
		public LFO Lfo1, Lfo2, Lfo3, Lfo4;
		public MidiInput MidiInput;
		public ModMatrix ModMatrix;

		// --------------------------- Parameters ----------------------------

		public double Osc1Vol, Osc2Vol, Filter1Vol, Filter2Vol;
		//public double Osc1Wave, Osc2Wave;

		// ----------------------------Voice Specific Parameters ----------------------------

		public Voice()
		{
			Osc1 = new WavetableOsc(48000);
			Filter1 = new LadderFilter(48000);
			MidiInput = new MidiInput();
			AmpEnv = new Ahdsr(48000);
			Filter1Env = new Ahdsr(48000);

			ModMatrix = new ModMatrix(this);

			// create default mod routes

			var ampRoute = new ModRouting();
			ampRoute.Source = ModSource.AmpEnv;
			ampRoute.Destination = ModDestination.Filter1Vol;
			ampRoute.Amount = 1.0;
			ModMatrix.Routes.Add(ampRoute);

			var filter1Route = new ModRouting();
			filter1Route.Source = ModSource.Filter1Env;
			filter1Route.Destination = ModDestination.Filter1Freq;
			filter1Route.Amount = 1.0;
			ModMatrix.Routes.Add(filter1Route);
		}

		int _note;
		public int Note
		{
			get { return _note; }
			set
			{
				_note = value;
				MidiInput.Pitch = value;
				Osc1.Pitch = value;
			}
		}

		double _gate;
		public double Gate
		{
			get { return _gate; }
			set
			{
				_gate = value;
				MidiInput.Gate = value;
				Filter1Vol = value;
				AmpEnv.Gate = value > 0.0;
				Filter1Env.Gate = value > 0.0;
			}
		}

		/// <summary>
		/// Tells if the voice is generating any audio
		/// </summary>
		public bool Active;

		int ProcessingIndex;
		int ModulationDownsample = 1;

		public double Process()
		{
			if (ProcessingIndex == 0)
			{
				Osc1Vol = Osc2Vol = Filter1Vol = Filter2Vol = 1.0;

				AmpEnv.AttackShape = 1.0;
				AmpEnv.SmoothTransition = true;
				AmpEnv.Retrigger = true;
				
				// Process modulation
				AmpEnv.Process(ModulationDownsample);
				Filter1Env.Process(ModulationDownsample);
				ModMatrix.Process();
			}

			ProcessingIndex = (ProcessingIndex + 1) % ModulationDownsample;

			double val = Osc1.Process();
			val = Filter1.Process(val);


			val = val * Filter1Vol * 0.5;// ((Gate > 0.01) ? 0.5 : 0.0);
			return val;
		}
	}
}
