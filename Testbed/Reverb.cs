using AudioLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Testbed
{
	class Reverb : TestContainer
	{
		Reverber Rev;

		public Reverb()
		{
			IsSynth = false;
			ParameterNames = new[] 
			{
				"Predelay", 
				"Size", 
				"Density", 
				"Decay", 
				"Delay", 
				"Hi Cut", 
				"Hi Cut Amt", 
				"AP-Delay", 
				"AP-Feedback", 
				"Mod Rate",
				"Mod Amount",
				"Late Stages",
				"Dry", 
				"Wet" 
			};

			Parameters = new double[ParameterNames.Length];
			Rev = new Reverber();
			Rev.Samplerate = 48000;
			Rev.SetTaps(100, 1000, 20);
		}

		public double Predelay   { get { return Parameters[0] * 100; } }
		public double Size       { get { return Parameters[1] * 400; } }
		public int Density       { get { return (int)(Parameters[2] * 100); } }
		public double Decay      { get { return Parameters[3]; } }
		public double Delay      { get { return Parameters[4] * 100; } }

		public double HiCut      { get { return ValueTables.Get(Parameters[5], ValueTables.Response2Dec) * 20000; } }
		public double HiCutAmt   { get { return Parameters[6]; } }

		public double APDelay    { get { return Parameters[7] * 100; } }
		public double APFeedback { get { return Parameters[8]; } }

		public double ModRate    { get { return Parameters[9] * 2; } }
		public double ModAmount  { get { return Parameters[10]; } }

		public int LateStages    { get { return 1 + (int)(Parameters[11] * 7.999); } }

		public double Dry        { get { return Parameters[12]; } }
		public double Wet        { get { return Parameters[13]; } }

		public override void Process()
		{
			var dry = Dry;
			var wet = Wet;
			for (int i = 0; i < Inputs[0].Length; i++)
			{
				Outputs[0][i] = dry * Inputs[0][i] + wet * Rev.Process(Inputs[0][i]);
				Outputs[1][i] = Outputs[0][i];
			}
		}

		public override void SetSamplerate(double rate)
		{
			Samplerate = rate;
			Rev.Samplerate = rate;
			Rev.SetTaps(Predelay, Size, Density);
		}

		public override string GetDisplay(int i)
		{
			if (i == 0) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", Predelay);
			if (i == 1) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", Size);
			if (i == 2) return String.Format(CultureInfo.InvariantCulture, "{0:0}x", Density);
			if (i == 3) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Decay);
			if (i == 4) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Delay);

			if (i == 5) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}Hz", HiCut);
			if (i == 6) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", HiCutAmt);

			if (i == 7) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", APDelay);
			if (i == 8) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", APFeedback);

			if (i == 9) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}Hz", ModRate);
			if (i == 10) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", ModAmount);

			if (i == 11) return String.Format(CultureInfo.InvariantCulture, "{0:0}", LateStages);

			if (i == 12) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Dry);
			if (i == 13) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Wet);
			return base.GetDisplay(i);
		}

		public override void ParameterUpdated(int i)
		{
			Rev.SetTaps(Predelay, Size, Density);
			Rev.SetLate(APFeedback, (int)(APDelay / 1000.0 * Samplerate));
			Rev.GlobalFeedback = Decay;
			Rev.GlobalDelay = (int)(Delay / 1000.0 * Samplerate);
			Rev.SetHiCut(HiCut, HiCutAmt);
			Rev.SetMod(ModRate, ModAmount);
			Rev.LateStages = LateStages;
		}
	}

	class Reverber
	{
		double _samplerate;
		public double Samplerate
		{
			get { return _samplerate; }
			set
			{
				_samplerate = value;
				foreach (var allpass in AllpassModules)
				{
					allpass.Samplerate = _samplerate;
					allpass.HiCut = allpass.HiCut;
				}
			}
		}

		public double GlobalFeedback { get; set; }
		public int GlobalDelay { get; set; }
		public int LateStages { get; set; }

		double[] Amplitudes;
		int[] Taps;
		Allpass[] AllpassModules;

		double[] EarlyBuffer;
		int EarlyI;

		double[] OutBuffer;
		int OutI;

		int SampleCount;

		public Reverber()
		{
			AllpassModules = new Allpass[8];
			for (int i = 0; i < AllpassModules.Length; i++)
			{
				AllpassModules[i] = new Allpass();
			}
			
			OutBuffer = new double[48000];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predelay">Predelay ms</param>
		/// <param name="size">Early Reflection size ms</param>
		public void SetTaps(double predelay, double size, int density)
		{
			if (size < 5)
				size = 5;
			if (density < 5)
				density = 5;

			var rand = new Random();
			var min = (int)(predelay / 1000.0 * Samplerate);
			var max = min + (int)(size / 1000.0 * Samplerate);
			Taps = Enumerable.Range(0, density).Select(x => rand.Next(min, max)).OrderBy(x => x).ToArray();
			Amplitudes = Taps.Select(x => Math.Exp(-x / (double)max * 3) * 2 * (0.5 - rand.NextDouble())).ToArray();
			//var gain = 1.0 / Amplitudes.Sum();
			//Amplitudes = Amplitudes.Select(x => x * gain).ToArray();
			EarlyBuffer = new double[max * 2];
			EarlyI = 0;
		}

		public void SetLate(double feedback, int allpassDelaySamples)
		{
			var rand = new Random();

			for (int i = 0; i < AllpassModules.Length; i++)
			{
				var ap = AllpassModules[i];
			
				ap.Feedback = feedback * (0.9 + 0.1 * rand.NextDouble());
				ap.Feedback = ap.Feedback > 0.98 ? 0.98 : ap.Feedback;
				ap.DelaySamples = (int)(allpassDelaySamples * (1 + 0.5 * i * (0.3 + 0.7 * rand.NextDouble())));
			}
		}

		public void SetHiCut(double fc, double amount)
		{
			var rand = new Random();
			foreach (var allpass in AllpassModules)
			{
				allpass.HiCut = fc * (0.5 + rand.NextDouble());
				allpass.HiCutAmount = amount;
			}
		}

		public void SetMod(double freq, double amount)
		{
			var rand = new Random();

			foreach (var allpass in AllpassModules)
			{
				allpass.ModFreq = freq * (1 + 0.8 * rand.NextDouble());
				allpass.ModAmount = amount;
			}

		}

		public double Process(double x)
		{
			SampleCount++;

			if (SampleCount % 16 == 0)
			{
				for (int i = 0; i < AllpassModules.Length; i++)
				{
					AllpassModules[i].UpdateMod(16);
				}
				SampleCount = 0;
			}
			
			var len = EarlyBuffer.Length;

			EarlyBuffer[EarlyI] = x;
			
			double outputEarly = 0.0;

			for (int i = 0; i < Taps.Length; i++)
			{
				var idx = (EarlyI + Taps[i]) % len;
				outputEarly += EarlyBuffer[idx] * Amplitudes[i];
			}

			var d = outputEarly + GlobalFeedback * OutBuffer[(OutI + GlobalDelay) % OutBuffer.Length];

			var stageCount = LateStages;
			for (int i = 0; i < stageCount; i++)
			{
				d = AllpassModules[i].Process(d);
			}
			
			OutBuffer[OutI] = d;

			EarlyI--;
			if (EarlyI < 0)
				EarlyI += len;

			OutI--;
			if (OutI < 0)
				OutI += OutBuffer.Length;

			return d;
		}
	}

	class Allpass
	{
		public double Samplerate;
		public double Feedback;
		public int DelaySamples;

		public double ModAmount;
		public double HiCutAmount;

		double _modFreq;
		public double ModFreq
		{
			get { return _modFreq; }
			set
			{
				_modFreq = value;
				ModIncrement = 1.0 / Samplerate * _modFreq;
			}
		}

		double _hiCut;
		public double HiCut
		{
			get { return _hiCut; }
			set
			{
				_hiCut = value;
				Alpha = Math.Exp(-2 * Math.PI * _hiCut / Samplerate);
			}
		}

		private double ModPhase;
		private double ModValue;
		private double ModIncrement;

		private double Alpha;
		private double[] Buffer;
		//private double[] BufferOut;
		private int I;

		private double A;
		private double AOut;

		public Allpass()
		{
			Buffer = new double[48000];
			Feedback = 0.7;
		}

		public void UpdateMod(int sampleCount)
		{
			ModPhase += sampleCount * ModIncrement;
			if (ModPhase > 1.0)
				ModPhase -= 1.0;

			ModValue = Math.Sin(ModPhase * 2 * Math.PI);
		}

		public double Process(double x)
		{
			var len = Buffer.Length;
			var k = (int)(I + DelaySamples * (1 + 0.01 * ModAmount * ModValue)) % len;

			var bufOut = Buffer[k];
			var bufIn = x - Feedback * bufOut;
			Buffer[I] = bufIn;
			var y = Feedback * bufIn + bufOut;

			I--;
			if (I < 0)
				I += len;

			A = (1 - Alpha) * y + Alpha * A;
			AOut = A * HiCutAmount + y * (1 - HiCutAmount);
			return AOut;
		}
	}
}
