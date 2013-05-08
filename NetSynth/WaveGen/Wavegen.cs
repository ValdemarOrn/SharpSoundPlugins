using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveGen
{
	public abstract class Wavegen
	{
		public List<double[]> Waves;

		public List<WaveParameter> Parameters;

		public int WaveCount;
		public int SampleCount;

		public abstract void Process();
	}
}
