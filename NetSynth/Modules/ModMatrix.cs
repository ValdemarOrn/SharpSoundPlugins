using AudioLib.Modules;
using NetSynth.Modulation;
using NetSynth.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.Modulation
{
	public enum ModDestination
	{
		None = 0,

		Osc1Pitch = 100,
		Osc1Vol,
		Osc2Pitch,
		Osc2Vol,

		Filter1Freq,
		Filter1Res,
		Filter1Vol,

		Filter2Freq,
		Filter2Res,
		Filter2Vol,

		Lfo1Speed,
		Lfo2Speed
	}

	public enum ModSource
	{
		None = 0,

		Pitch = 200,
		Velocity,
		ModWheel,

		AmpEnv,
		Filter1Env,
		Filter2Env,
		ModEnv1,
		ModEnv2,
		ModEnv3,
		ModEnv4,
		Lfo1,
		Lfo2

	}

	public static class Mod
	{
		public static string Name(this ModDestination src)
		{
			switch (src)
			{
				case (ModDestination.Osc1Pitch):
					return "Osc. 1 Pitch";
				case (ModDestination.Osc2Pitch):
					return "Osc. 2 Pitch";
				case (ModDestination.Filter1Freq):
					return "Filter 1 Freq.";
				case (ModDestination.Filter1Res):
					return "Filter 1 Res.";
				case (ModDestination.Filter1Vol):
					return "Filter 1 Amp.";
				case (ModDestination.Filter2Freq):
					return "Fitler 2 Freq.";
				case (ModDestination.Filter2Res):
					return "Filter 2 Res.";
				case (ModDestination.Filter2Vol):
					return "Filter 2 Amp.";
				case (ModDestination.Lfo1Speed):
					return "LFO 1 Speed";
				case (ModDestination.Lfo2Speed):
					return "LFO 2 Speed";
			}

			return "";
		}

		public static string Name(this ModSource src)
		{
			switch(src)
			{
				case(ModSource.Pitch):
					return "Pitch";
				case (ModSource.Velocity):
					return "Velocity";
				case (ModSource.ModWheel):
					return "ModWheel";
				case (ModSource.AmpEnv):
					return "Amplifier Envelope";
				case (ModSource.Filter1Env):
					return "Filter 1 Envelope";
				case (ModSource.Filter2Env):
					return "Filter 2 Envelope";
				case (ModSource.ModEnv1):
					return "Mod. Envelope 1";
				case (ModSource.Lfo1):
					return "LFO 1";
				case (ModSource.Lfo2):
					return "LFO 2";
			}

			return "";
		}
	}

	public sealed class ModRouting
	{
		public ModSource Source;
		public ModDestination Destination;

		public double Amount;

		/// <summary>
		/// True if visible in GUI
		/// </summary>
		public bool Visible;
	}
}

namespace NetSynth.Modules
{
	public sealed class ModMatrix
	{
		// -------------------- Modules --------------------

		public Voice Voice;


		public List<ModRouting> Routes;

		public static int[] AvailableSources;
		public static int[] AvailableDestinations;

		static ModMatrix()
		{
			var sources = new List<int>();
			var destinations = new List<int>();
			
			foreach (var s in Enum.GetValues(typeof(ModDestination)))
				destinations.Add((int)s);

			foreach (var s in Enum.GetValues(typeof(ModSource)))
				sources.Add((int)s);

			AvailableSources = sources.ToArray();
			AvailableDestinations = destinations.ToArray();
		}

		public ModMatrix(Voice voice)
		{
			Voice = voice;
			Routes = new List<ModRouting>();
		}

		public void Process()
		{
			// Note: don't use foreach because another thread change modify the collection during the looping
			for(int i=0; i < Routes.Count; i++)
			{
				// one in a billion chance the user can remove the last route at the very momeny we try to read it
				// highly improbably, but if this code causes exceptions, it'll probably be that
				// probably requires locking to get rid of, or replacing the old route with null and performing null check
				var route = Routes[i];

				double src = 0.0;
				double amt = route.Amount;

				switch (route.Source)
				{
					case(ModSource.AmpEnv):
						src = Voice.AmpEnv.Output;
						break;
					case(ModSource.Filter1Env):
						src = Voice.Filter1Env.Output;
						break;
					case (ModSource.Filter2Env):
						src = Voice.Filter2Env.Output;
						break;
					case (ModSource.Lfo1):
						src = Voice.Lfo1.Output;
						break;
					case (ModSource.Lfo2):
						src = Voice.Lfo2.Output;
						break;
					case (ModSource.ModEnv1):
						src = Voice.ModEnv1.Output;
						break;
					case (ModSource.ModWheel):
						src = 0.0;
						break;
					case (ModSource.Pitch):
						src = 0.0;
						break;
					case (ModSource.Velocity):
						src = 0.0;
						break;
				}

				switch(route.Destination)
				{
					case (ModDestination.Filter1Vol):
						Voice.Filter1Vol = Voice.Filter1Vol * (amt * src + (1 - amt));
						break;
					case (ModDestination.Filter1Freq):
						Voice.Filter1.Cutoff += src * amt;
						break;
					case (ModDestination.Filter1Res):
						Voice.Filter1.Resonance += src * amt;
						break;

					case (ModDestination.Filter2Vol):
						Voice.Filter2Vol = Voice.Filter2Vol * (amt * src + (1 - amt));
						break;
					case (ModDestination.Filter2Freq):
						Voice.Filter2.Cutoff += src * amt;
						break;
					case (ModDestination.Filter2Res):
						Voice.Filter2.Resonance += src * amt;
						break;

					case (ModDestination.Lfo1Speed):
						Voice.Lfo1.FreqHz += src * amt * 15.0; // full modulation gives +-15Hz swing
						break;
					case (ModDestination.Lfo2Speed):
						Voice.Lfo2.FreqHz += src * amt * 15.0; // full modulation gives +-15Hz swing
						break;

					case (ModDestination.Osc1Pitch):
						Voice.Osc1.Pitch += src * amt * 36.0; // full modulation gives +-3 octaves
						break;
					case (ModDestination.Osc2Pitch):
						Voice.Osc2.Pitch += src * amt * 36.0; // full modulation gives +-3 octaves
						break;
				}
			}
		}
	}
}
