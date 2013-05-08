using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveGen
{
	public class SawtoothGen : Wavegen
	{
		public SawtoothGen()
		{
			Parameters = new List<WaveParameter>();
			Parameters.Add(new WaveParameter() { Name = "Skew" });
			Parameters.Add(new WaveParameter() { Name = "Self Sync" });
		}

		public override void Process()
		{
			var waves = new List<double[]>();

			for(int n = 0; n < WaveCount; n++)
			{
				var wave = new double[SampleCount];
				waves.Add(wave);
				double power = 1 + Parameters[0].GetValue(n, WaveCount) * 10;
				double selfSync = 1 + Parameters[1].GetValue(n, WaveCount) * 8;

				for(int i = 0; i < SampleCount; i++)
				{
					double pos = i / (double)SampleCount;
					pos = Math.Pow(pos, power);
					double value = (pos * selfSync) % 1.0;
					value = 1 - 2 * value;
					wave[i] = value;
					
					
				}
			}

			this.Waves = waves;
		}
	}

}
