using AudioLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testbed
{
	class BLInt : TestContainer
	{
		public BLInt()
		{
			IsSynth = true;
			Parameters = new double[1];
			ParameterNames = new string[] { "Filter" };
			Parameters[0] = 0.5;
			Wave = Utils.Square(1024, 1.0, 0.5);
			//Sinc = Utils.Sinc(0.01, 2000); // 1 unit => 50 samples
		}

		double Pos;
		double Increment;
		double[] Wave;
		//double[] Sinc;

		public override void Process()
		{
			Note note = new Note(0, 0);
			if (Notes.Count == 0)
			{
				for (int i = 0; i < Inputs[0].Length; i++)
				{
					Outputs[0][i] = 0;
					Outputs[1][i] = 0;
				}

				return;
			}
			
			note = Notes[Notes.Count - 1];

			Increment = CalculateIncrement(note.Pitch);
			var f = Parameters[0];

			try
			{
				for (int i = 0; i < Inputs[0].Length; i++)
				{
					var lookup = (1 - f) * Pos + f * ValueTables.Get(Pos / Wave.Length, ValueTables.Pow3) * Wave.Length;
					var sample = LinInterp(Wave, lookup);
					Pos += Increment;
					if (Pos >= Wave.Length)
						Pos -= Wave.Length;

					Outputs[0][i] = sample * note.Velocity * 0.007874;
					Outputs[1][i] = sample * note.Velocity * 0.007874;
				}
			}
			catch(Exception)
			{
			}
		}

		//private double GetBLSample(double f)
		//{
		//	var sincStep = 50 * f;
		//	double output = LinInterp(Wave, Pos);
		//	int max = (int)(2000 / sincStep);
		//	if (max > 100)
		//		max = 100;

		//	for(int i = 1; i < max; i++)
		//	{
		//		double w1 = LinInterp(Wave, Pos - Increment * i);
		//		double w2 = LinInterp(Wave, Pos + Increment * i);
		//		double s = LinInterp(Sinc, 2000 + i * sincStep, false);
		//		output += w1 * s + w2 * s;
		//	}

		//	return output;
		//}

		private double LinInterp(double[] wave, double position, bool wrap = true)
		{
			if(wrap)
				position = (1000000 * wave.Length + position) % wave.Length;

			var ia = (int)position;
			var ib = (int)(position + 1);
			if (ib >= wave.Length && wrap)
				ib = 0;
			else if (ib >= wave.Length)
				ib = ia;

			var dx = position % 1.0;
			return (1 - dx) * wave[ia] + dx * wave[ib];
		}

		private double CalculateIncrement(int pitch)
		{
			var hz = Utils.Note2HzLookup(pitch);
			var samplesPerCycle = Samplerate / hz;
			return Wave.Length / samplesPerCycle;
		}

		public override void SetSamplerate(double rate)
		{
			Samplerate = rate;
		}
	}
}
