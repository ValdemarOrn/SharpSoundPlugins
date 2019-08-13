using AudioLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomDiffuser
{
	class DelayConfig
	{
		public double PreDelay;
		public double Delay;
		public double DelayScatter;
		public double PitchScatter;
		public double PanIntensity;
		public double Length;
		public double LengthScatter;

		public int Seed;
		public int ActiveGrains;
		public double Mix;

		public DelayConfig()
		{
			PreDelay = 200;
			Delay = 2000;
			DelayScatter = 10;
			PitchScatter = 0.0;
			PanIntensity = 0.5;
			Length = 24000;
			LengthScatter = 1;
		}
	}

	class GrainDelay
	{
		public const int MaxGrains = 100;
		public const int InputBufferSize = 192000;

		public DelayConfig Config;

		public double[] DelaySamples;
		public double[] IncrementOffset;
		public double[] LenSamples;
		public double[] PhaseSamples;
		public double[] InputPan;
		public double[] OutputPan;
		public double[] Gain;

		private int InputBufferIdx;
		private double[][] InputBuffer;

		Random rand;
		public double Random(double min, double max) => rand.NextDouble() * (max - min) + min;

		public GrainDelay()
		{
			Config = new DelayConfig();
			DelaySamples = new double[MaxGrains];
			IncrementOffset = new double[MaxGrains];
			LenSamples = new double[MaxGrains];
			PhaseSamples = new double[MaxGrains];
			InputPan = new double[MaxGrains];
			OutputPan = new double[MaxGrains];
			Gain = new double[MaxGrains];

			InputBuffer = new[] { new double[InputBufferSize], new double[InputBufferSize] };
			rand = new Random();

			Configure();
		}

		public void Process(double[][] inData, int readIndex, double[][] outData, int writeIndex)
		{
			InputBufferIdx--;
			if (InputBufferIdx < 0)
				InputBufferIdx += InputBufferSize;

			var mix = 1.0 / Math.Sqrt(Math.Sqrt(Config.ActiveGrains)) * Config.Mix;
			var dry = 1 - Config.Mix;

			InputBuffer[0][InputBufferIdx] = inData[0][readIndex];
			InputBuffer[1][InputBufferIdx] = inData[1][readIndex];

			// dry signal
			outData[0][writeIndex] = inData[0][readIndex] * dry;
			outData[1][writeIndex] = inData[1][readIndex] * dry;

			unsafe
			{
				for (int grainId = 0; grainId < Config.ActiveGrains; grainId++)
				{
					if (PhaseSamples[grainId] >= LenSamples[grainId])
						Reset(grainId);
				}

				var readOffset = new double[MaxGrains];
				var sample = new double[MaxGrains];
				var out_sample = new double[MaxGrains];

				for (int grainId = 0; grainId < Config.ActiveGrains; grainId++)
				{
					readOffset[grainId] = DelaySamples[grainId] - PhaseSamples[grainId] * IncrementOffset[grainId];
					if (readOffset[grainId] < 0)
					{
						// reading form the future, happens with low delay and high amount of positive IncrementOffset
						readOffset[grainId] = 0;
					}
				}

				for (int grainId = 0; grainId < Config.ActiveGrains; grainId++)
				{
					sample[grainId] = GetSample(grainId, readOffset[grainId]);
					out_sample[grainId] = Window(grainId, sample[grainId]) * Gain[grainId];

					outData[0][writeIndex] += out_sample[grainId] * (1 - OutputPan[grainId]) * mix;
					outData[1][writeIndex] += out_sample[grainId] * (OutputPan[grainId]) * mix;

					PhaseSamples[grainId] += 1;
				}
			}
		}

		private double GetSample(int grainId, double offset)
		{
			unsafe
			{
				// implements modululo, wrapping around the InputBufferSize
				var idx = (InputBufferIdx + offset);
				var isOverflow = idx > InputBufferSize;
				var isOverflowInt = *(byte*)&isOverflow;
				idx = idx - isOverflowInt * InputBufferSize;

				var ia = (int)idx;
				var ib = (ia + 1) % InputBufferSize;
				var frac = idx - ia;
				
				var r = InputPan[grainId];
				var l = 1 - r;

				var left = InputBuffer[0][ia] * (1 - frac) + InputBuffer[0][ib] * frac;
				var right = InputBuffer[1][ia] * (1 - frac) + InputBuffer[1][ib] * frac;

				return left * l + right * r;
			}
		}

		private double Window(int grainId, double sample)
		{
			var x = PhaseSamples[grainId] / LenSamples[grainId];
			//return sample * Math.Sin(x * Math.PI);
			var k = x * 2 - 1;
			return (1 - k * k) * sample; // sin approx
		}

		public void Configure()
		{
			rand = new Random(Config.Seed);

			for (int grainId = 0; grainId < Config.ActiveGrains; grainId++)
			{
				double len = (int)Random(Config.Length, Config.Length * Config.LengthScatter);
				var pitchScatter = 0.05 * ValueTables.Get(Config.PitchScatter, ValueTables.Pow2);
				var delay = Random(Config.Delay, Config.Delay * Config.DelayScatter);
				var gain = Math.Pow(1.6, -delay / Config.Delay);

				DelaySamples[grainId] = Config.PreDelay + delay;
				IncrementOffset[grainId] = Random(-pitchScatter, pitchScatter);
				Gain[grainId] = gain;

				// prevent reading the future by increasing the delay for heavily positively modulated grains
				if (IncrementOffset[grainId] * LenSamples[grainId] > DelaySamples[grainId])
					DelaySamples[grainId] = IncrementOffset[grainId] * LenSamples[grainId] + 5;

				if (grainId % 2 == 0)
				{
					LenSamples[grainId] = len;
					PhaseSamples[grainId] = (grainId / (double)Config.ActiveGrains) * LenSamples[grainId];
					InputPan[grainId] = rand.Next(0, 2);
					OutputPan[grainId] = rand.Next(0, 2);
				}
				else
				{
					len = LenSamples[grainId - 1];
					LenSamples[grainId] = len;
					PhaseSamples[grainId] = (PhaseSamples[grainId - 1] + len / 2) % len;
					InputPan[grainId] = InputPan[grainId - 1];
					OutputPan[grainId] = OutputPan[grainId - 1];
				}
			}
		}

		public void Reset(int grainId)
		{
			PhaseSamples[grainId] -= LenSamples[grainId];
		}
	}
}
