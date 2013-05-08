using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveGen
{
	public class WaveParameter
	{
		public WaveParameter()
		{
			Formatter = (x) => String.Format("{0:0.000}", x);
			Converter = x => x;
		}

		public string Name;

		/// <summary>
		/// Range 0...1
		/// </summary>
		public double ValueStart;

		/// <summary>
		/// Range 0...1
		/// </summary>
		public double ValueFinish;

		/// <summary>
		/// Used to convert value range 0...1 to something else
		/// </summary>
		public Func<double, double> Converter;

		public Func<double, string> Formatter;

		public double GetValue(int wavePos, int waveCount)
		{
			double pos = wavePos / (double)(waveCount - 1);

			double a = Converter(ValueStart);
			double b = Converter(ValueFinish);

			double output = (a * (1 - pos) + b * pos);
			return output;
		}

		public string GetString(int wavePos, int waveCount)
		{
			return Formatter(GetValue(wavePos, waveCount));
		}
	}
}
