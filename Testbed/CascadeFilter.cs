using AudioLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testbed
{
	class CascadeFilter : TestContainer
	{
		public CascadeFilter()
		{
			Filter = new LadderFilter(48000);
			IsSynth = false;
			Parameters = new double[12];
			ParameterNames = new string[] { "Cutoff", "Res", "X", "A", "B", "C", "D", "A HP", "B HP", "C HP", "D HP", "Cutoff2" };
			Parameters[0] = 0.5;
			Parameters[1] = 0.0;

			Parameters[2] = 0.5;
			Parameters[3] = 0.5;
			Parameters[4] = 0.5;
			Parameters[5] = 0.5;
			Parameters[6] = 0.9;
		}

		LadderFilter Filter;

		public override void Process()
		{
			Filter.Cutoff = Parameters[0];
			Filter.Resonance = Parameters[1];

			Filter.VX = (Parameters[2] - 0.5) * 2;
			Filter.VA = (Parameters[3] - 0.5) * 2;
			Filter.VB = (Parameters[4] - 0.5) * 2;
			Filter.VC = (Parameters[5] - 0.5) * 2;
			Filter.VD = (Parameters[6] - 0.5) * 2;

			Filter.HpA = Parameters[7] > 0.5;
			Filter.HpB = Parameters[8] > 0.5;
			Filter.HpC = Parameters[9] > 0.5;
			Filter.HpD = Parameters[10] > 0.5;

			Filter.Cutoff2 = Parameters[11];

			for (int i = 0; i < Inputs[0].Length; i++)
			{
				Outputs[0][i] = Filter.Process(Inputs[0][i]);
				Outputs[1][i] = Outputs[0][i];
			}
		}

		public override void SetSamplerate(double rate)
		{
			Samplerate = rate;
			Filter.Samplerate = rate;
		}

		public override string GetDisplay(int i)
		{
			if (i >= 2 && i <= 6)
				return String.Format("{0:0.00}", Parameters[i] * 2 - 1);
			else
				return base.GetDisplay(i);
		}
	}


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

				_cutoff = 10 + value * value * 24000;

				double x = 2 * Math.PI * _cutoff / _fs;

				pCutA = (2 - Math.Cos(x)) - Math.Sqrt(Math.Pow(2 - Math.Cos(x), 2) - 1);

				//pLP = (1 - 2 * Cutoff / _fs) * (1 - 2 * Cutoff / _fs);
				//pHP = (2 * Cutoff / _fs) * (2 * Cutoff / _fs);
			}
		}

		double _cutoff2;
		public double Cutoff2
		{
			get { return _cutoff2; }
			set
			{
				if (value < 0.0)
					value = 0.0;
				else if (value > 1.0)
					value = 1.0;

				// x * 0.03 + x^6 * 0.97  ~ 3 Decade response
				double n = value;
				double n3 = value * value * value;
				double n6 = n3 * n3;
				double v = n * 0.03 + n6 * 0.97;

				_cutoff2 = 10 + v * 24000;

				double x = 2 * Math.PI * _cutoff2 / _fs;

				pCutB = (2 - Math.Cos(x)) - Math.Sqrt(Math.Pow(2 - Math.Cos(x), 2) - 1);

				//pLP = (1 - 2 * Cutoff / _fs) * (1 - 2 * Cutoff / _fs);
				//pHP = (2 * Cutoff / _fs) * (2 * Cutoff / _fs);
			}
		}

		double pCutA;
		double pCutB;

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

		public bool HpA;
		public bool HpB;
		public bool HpC;
		public bool HpD;

		double x = 0;
		double ALow = 0;
		double BLow = 0;
		double CLow = 0;
		double DLow = 0;

		double AHi = 0;
		double BHi = 0;
		double CHi = 0;
		double DHi = 0;

		double AOut = 0;
		double BOut = 0;
		double COut = 0;
		double DOut = 0;

		double Feedback = 0;

		public double Process(double input)
		{
			for (int i = 0; i < oversample; i++)
			{
				double fb = Resonance * 4 * (Feedback - 0.5 * input);
				double val = input - fb;
				x = val;

				

				// stage1
				if (HpA)
					ALow = (1 - pCutB) * val + pCutB * ALow;
				else
					ALow = (1 - pCutA) * val + pCutA * ALow;
				AHi = val - ALow;

				AOut = (HpA) ? AHi : ALow;
				val = AOut;

				// stage2
				if (HpB)
					BLow = (1 - pCutB) * val + pCutB * BLow;
				else
					BLow = (1 - pCutA) * val + pCutA * BLow;
				BHi = val - BLow;

				BOut = (HpB) ? BHi : BLow;
				val = BOut;

				// stage3
				if (HpC)
					CLow = (1 - pCutB) * val + pCutB * CLow;
				else
					CLow = (1 - pCutA) * val + pCutA * CLow;
				CHi = val - CLow;

				COut = (HpC) ? CHi : CLow;
				val = COut;

				// stage4
				if(HpD)
					DLow = (1 - pCutB) * val + pCutB * DLow;
				else
					DLow = (1 - pCutA) * val + pCutA * DLow;
				DHi = val - DLow;

				DOut = (HpD) ? DHi : DLow;
				val = DOut;

				Feedback = Utils.TanhLookup(val);
			}

			Output = (VX * x + VA * AOut + VB * BOut + VC * COut + VD * DOut);
			return Output;
		}

		public double Output;
	}
}
