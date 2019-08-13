using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomDiffuser
{
	class Grain
	{
		private static Random rand = new Random();
		private static readonly Func<double, double, double> random = (min, max) => rand.NextDouble() * (max - min) + min;
		private double[] FeedbackBuffer;
		private int fbBufferIdx;

		public double DelaySamples;
		public double IncrementOffset;
		public double Pan;
		public double LenSamples;
		public double Mix;
		public double PhaseSamples = 0.0;
		public double InputLeft;
		public double InputRight;
		public double Feedback;
		public Grain()
		{
			FeedbackBuffer = new double[32000];
			Init();
		}
		
		public void Process(double[][] input, int readIndex, double[][] output, int writeIndex)
		{
			if (PhaseSamples >= LenSamples)
			{
				Init();
				Reset();
			}

			var readIdx = readIndex - DelaySamples + PhaseSamples * IncrementOffset;
			if (readIdx > readIndex)
				return; // don't read from the future

			var sample = GetSample(input, readIdx);
			var out_sample = Window(sample) * Mix;

			FeedbackBuffer[fbBufferIdx] = out_sample * Feedback;
			fbBufferIdx = (fbBufferIdx + 1) % FeedbackBuffer.Length;

			output[0][writeIndex] += out_sample * Pan;
			output[1][writeIndex] += out_sample * (1 - Pan);

			PhaseSamples += 1;
		}

		private double Window(double sample)
		{
			var x = PhaseSamples / LenSamples;
			return sample * Math.Sin(x * Math.PI);
		}

		private double GetSample(double[][] input, double readIdx)
		{
			var bufLen = input[0].Length;
			var idx = (readIdx + 1000 * bufLen) % bufLen;
			var ia = (int)idx;
			var ib = (ia + 1) % bufLen;
			var frac = idx - ia;

			var left = input[0][ia] * (1 - frac) + input[0][ib] * frac;
			var right = input[1][ia] * (1 - frac) + input[1][ib] * frac;
			
			// feedback buffer read
			bufLen = FeedbackBuffer.Length;
			idx = ((fbBufferIdx - DelaySamples*4) + 1000 * bufLen) % bufLen;
			ia = (int)idx;
			ib = (ia + 1) % bufLen;
			var fb = FeedbackBuffer[ia] * (1 - frac) + FeedbackBuffer[ib] * frac;

			return left + right;
		}

		public void Init()
		{
			DelaySamples = random(500, 6000);
			IncrementOffset = random(-0.001, 0.001);
			LenSamples = random(10000, 30000);
			PhaseSamples = random(0, 1) * LenSamples;
			InputLeft = rand.Next(0, 2);
			InputRight = 1 - InputLeft;
			Pan = random(0, 1);
			Pan = Pan * Pan;
			Feedback = 0.95;
		}

		public void Reset()
		{
			PhaseSamples = 0.0;
		}
	}
}
