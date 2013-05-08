using AudioLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveGen
{
	public class UltrabassGen : Wavegen
	{
		public UltrabassGen()
		{
			Parameters = new List<WaveParameter>();
			Parameters.Add(new WaveParameter() { Name = "Slope" });
		}

		public override void Process()
		{
			var waves = new List<double[]>();

			for (int n = 0; n < WaveCount; n++)
			{
				double angle = (Math.PI * 0.25) + Math.Sqrt(Parameters[0].GetValue(n, WaveCount)) * Math.PI * 0.2499;
				double x = 1 / Math.Tan(angle);
				if (x > 1 || Double.IsInfinity(x) || Double.IsNaN(x))
					x = 1;

				double[] partials = new double[64];

				for (int i = 1; i < partials.Length; i++)
				{
					partials[i] = 1 - (i-1) * x;
					if (partials[i] < 0.0)
						partials[i] = 0.0;
				}

				var signal = SimpleDFT.IDFT(new Pair<double[], double[]>(partials, new double[partials.Length]), 2048);

				Utils.AddInPlace(signal, -Utils.Average(signal));
				Utils.GainInPlace(signal, 1.0 / Utils.Max(signal));

				waves.Add(signal);
			}

			this.Waves = waves;
		}
	}

}
