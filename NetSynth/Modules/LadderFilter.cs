using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioLib;
using AudioLib.TF;

namespace NetSynth.Modules
{
	public sealed class LadderFilter
	{
		double oversample = 4;
		double _fs;
		public double Samplerate
		{
			get { return _fs; }
			set
			{
				_fs = oversample * value;
			}
		}

		double _cutoff;
		public double Cutoff
		{
			get { return _cutoff; }
			set
			{
				if (value < 0.0)
					value = 0.0;
				else if (value > 1.0)
					value = 1.0;

				_cutoff = 10 + ValueTables.Get(value, ValueTables.Response3Dec) * 24000;
				pCoefficient = (1 - 2 * Cutoff / Samplerate) * (1 - 2 * Cutoff / Samplerate);
			}
		}

		double pCoefficient;

		public double Resonance;

		public LadderFilter(double samplerate)
		{
			this.Samplerate = samplerate;
		}

		public double VX;
		public double VA;
		public double VB;
		public double VC;
		public double VD;

		double x = 0;
		double a = 0;
		double b = 0;
		double c = 0;
		double d = 0;
		double Feedback = 0;

		public double Process(double input)
		{
			// Low Pass
			var p = pCoefficient;

			for (int i = 0; i < oversample; i++)
			{
				double fb = Resonance * 4 * (Feedback - 0.5 * input);
				double val = input - fb;
				x = val;

				// low pass
				
				a = (1 - p) * val + p * a;
				val = a;
				b = (1 - p) * val + p * b;
				val = b;
				c = (1 - p) * val + p * c;
				val = c;
				d = (1 - p) * val + p * d;
				val = d;

				Feedback = Utils.TanhLookup(val);
			}

			Output = (VX * x + VA * a + VB * b + VC * c + VD * d);
			return Output;
		}

		public double Output;
	}
}

