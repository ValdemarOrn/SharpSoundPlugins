using AudioLib;
using AudioLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testbed
{
	class Resonator : TestContainer
	{
		public Resonator()
		{
			IsSynth = false;
			Parameters = new double[16];
			ParameterNames = new string[] { "Gain", "Volume", "Decay", "Base", "Pitch 2", "Pitch 3", "Pitch 4", "Pitch 5", "Vol 1", "Vol 2", "Vol 3", "Vol 4", "Vol 5", "Dry/Wet", "Filter", "Mode" };
			Parameters[0] = 0.5;
			Parameters[1] = 0.0;
		}

		Biquad Filter = new Biquad(Biquad.FilterType.LowPass, 48000);
		CircularBuffer Buf1 = new CircularBuffer(48000);
		CircularBuffer Buf2 = new CircularBuffer(48000);
		CircularBuffer Buf3 = new CircularBuffer(48000);
		CircularBuffer Buf4 = new CircularBuffer(48000);
		CircularBuffer Buf5 = new CircularBuffer(48000);

		public override void Process()
		{
			var gain = Utils.DB2gain(-30 + 48 * Parameters[0]);
			var volume = Utils.DB2gain(-60 + 66 * Parameters[1]);
			var decay = (1 - ValueTables.Get(1 - Parameters[2], ValueTables.Response3Oct)) * 1.14;
			var basePitch = 60 + (int)((Parameters[3] - 0.5) * 24.01);
			var p1 = basePitch;
			var p2 = basePitch + (int)((Parameters[4] - 0.5) * 24.01);
			var p3 = basePitch + (int)((Parameters[5] - 0.5) * 24.01);
			var p4 = basePitch + (int)((Parameters[6] - 0.5) * 24.01);
			var p5 = basePitch + (int)((Parameters[7] - 0.5) * 24.01);
			var vol1 = Utils.DB2gain(-30 + 36 * Parameters[8]);
			var vol2 = Utils.DB2gain(-30 + 36 * Parameters[9]);
			var vol3 = Utils.DB2gain(-30 + 36 * Parameters[10]);
			var vol4 = Utils.DB2gain(-30 + 36 * Parameters[11]);
			var vol5 = Utils.DB2gain(-30 + 36 * Parameters[12]);
			var wet = Parameters[13];
			var dry = 1 - wet;
			var cutoff = ValueTables.Get(Parameters[14], ValueTables.Response4Dec) * 22000;
			var mode = Parameters[15] < 0.5;

			var delay1 = GetSamplesPerNote(p1);
			var delay2 = GetSamplesPerNote(p2);
			var delay3 = GetSamplesPerNote(p3);
			var delay4 = GetSamplesPerNote(p4);
			var delay5 = GetSamplesPerNote(p5);

			Filter.Frequency = cutoff;
			Filter.Q = 0.7;
			Filter.Type = mode ? Biquad.FilterType.LowPass : Biquad.FilterType.HighPass;
			Filter.Update();

			for (int i = 0; i < Inputs[0].Length; i++)
			{
				var input = Filter.Process(Inputs[0][i]) * gain;
				var d1 = Buf1.Read(-delay1) * decay;
				var d2 = Buf2.Read(-delay2) * decay;
				var d3 = Buf3.Read(-delay3) * decay;
				var d4 = Buf4.Read(-delay4) * decay;
				var d5 = Buf5.Read(-delay5) * decay;

				var signal = d1 * vol1 + d2 * vol2 + d3 * vol3 + d4 * vol4 + d5 * vol5;

				Outputs[0][i] = (dry * Inputs[0][i] + wet * signal * 0.2) * volume;
				Outputs[1][i] = Outputs[0][i];
				Buf1.Write(Math.Tanh(input + d1));
				Buf2.Write(Math.Tanh(input + d2));
				Buf3.Write(Math.Tanh(input + d3));
				Buf4.Write(Math.Tanh(input + d4));
				Buf5.Write(Math.Tanh(input + d5));
			}
		}

		private void ResetAll()
		{
			for(int i = 0; i < 48000; i++)
			{
				Buf1.Write(0.0);
				Buf2.Write(0.0);
				Buf3.Write(0.0);
				Buf4.Write(0.0);
				Buf5.Write(0.0);
			}
		}

		int GetSamplesPerNote(int note)
		{
			var hz = Utils.Note2HzLookup(note);
			var samples = (int)(Math.Round(Samplerate / hz));
			return samples;
		}

		public override void SetSamplerate(double rate)
		{
			Samplerate = rate;
			Filter.Samplerate = rate;
		}
	}
}
