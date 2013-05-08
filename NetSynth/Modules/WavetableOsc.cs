using AudioLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.Modules
{
	public sealed class WavetableOsc : NetSynth.Modules.IOscillator
	{
		/// <summary>
		/// Radians per sample, from 0...0.5
		/// </summary>
		double FreqRad;

		public int[] PartialsDistribution;

		/// <summary>
		/// look-up table for waves, by number of partials
		/// </summary>
		public double[][][] Waves { get; set; }

		/// <summary>
		/// Number of current partials, used to find the correct wavetable in Waves.
		/// Depends on the pitch
		/// </summary>
		private int CurrentPartials;

		/// <summary>
		/// The wave index, used to find the correct wvetable in Waves
		/// </summary>
		double _currWaveIdx;
		public double CurrentWaveIndex
		{
			get { return _currWaveIdx; }
			set
			{
				if (value >= (WaveCount - 1))
					value = (WaveCount - 1) - 0.0001;

				_currWaveIdx = value;
			}
		}

		static int WaveCount = 32;
		static int SampleCount = 2048;

		public double Phase;

		private double _fs;
		private double _fsInv;
		private double _nyquist;
		private double _nyquistInv;

		public double Samplerate
		{
			get { return _fs; }
			set
			{ 
				_fs = value; 
				_fsInv = 1.0 / _fs;
				_nyquist = value / 2.0;
				_nyquistInv = 1 / _nyquist;
			}
		}

		int maxPartials = 512;

		public WavetableOsc(double samplerate)
		{
			Samplerate = samplerate;
			Waves = new double[WaveCount][][];
			PartialsDistribution = GetMaxPartials(maxPartials, 17000, 24000).ToArray();
			
			// initialize with empty wavetables
			for(int i = 0; i < Waves.Length; i++)
			{
				SetWave(new double[128], i);
			}

			CurrentPartials = 1;
		}

		public void SetWave(double[] wave, int waveIndex)
		{
			var waves = new double[maxPartials + 1][];
			var fftData = PadFFT(wave);
			Exocortex.DSP.Fourier.FFT(fftData, wave.Length, Exocortex.DSP.FourierDirection.Forward);

			// do ifft
			foreach(var partials in PartialsDistribution)
			{
				float[] partialsCopy = LimitPartials(fftData, partials);
				Exocortex.DSP.Fourier.FFT(partialsCopy, wave.Length, Exocortex.DSP.FourierDirection.Forward);
				waves[partials] = UnpadFFT(partialsCopy);
			}

			// fill in the gaps in the waves table
			for (int i = 0; i < waves.Length; i++)
			{
				if (waves[i] == null && i != 0)
					waves[i] = waves[i - 1];
			}

			Waves[waveIndex] = waves;
		}

		/// <summary>
		/// Takes in all the partials from the fft and deletes all the partials above the specified maximum
		/// Returns a modified copy of the input array
		/// </summary>
		/// <param name="fftData"></param>
		/// <param name="partials"></param>
		/// <returns></returns>
		private float[] LimitPartials(float[] fftData, int partials)
		{
			var data = new float[fftData.Length];
			Array.Copy(fftData, data, data.Length);
			int halfWay = data.Length / 2;

			for(int i = 0; i < data.Length; i++)
			{
				if(i > (partials * 2 + 2) && data.Length - i > 2 * partials)
					data[i] = 0;
			}

			return data;
		}

		/// <summary>
		/// Takes an input signal and pads it with zeros between every sample. Used for the fft operation
		/// (zeros indicate phase in a complex number)
		/// </summary>
		/// <param name="wave"></param>
		/// <returns></returns>
		private float[] PadFFT(double[] wave)
		{
			var data = new float[wave.Length * 2];
			for (int i = 0; i < wave.Length; i++)
				data[i * 2] = (float)wave[i];

			return data;
		}

		/// <summary>
		/// Removes the phase componenets from the output signal of an IFFT operation.
		/// (btw: Phase should be close to zero for real-valued input signals in the fft)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private double[] UnpadFFT(float[] data)
		{
			var wave = new double[data.Length / 2];
			double scale = 1.0 / wave.Length;
			for (int i = 0; i < wave.Length; i++)
				wave[i] = data[i * 2] * scale;

			return wave;
		}

		private void SetFrequencyHz(double hz)
		{
			this.FreqRad = hz * _fsInv;  // 500hz / 48000

			int partials = 1;
			if(hz > 0)
				partials = (int)(_nyquist / hz);

			if (partials > PartialsDistribution[0])
				partials = PartialsDistribution[0];

			CurrentPartials = partials;
		}
		
		/// <summary>
		/// Set the frequency by pitch
		/// Pitch of the note. 69.0f = A4 = 440Hz = Midi note 69. 
		/// Pitch increases 1 octave for +12.0. 
		/// Note: Negative values are allowed		
		/// </summary>
		private void SetFrequencyPitch(double value)
		{
			value = value + _octave * 12 + _semi + _cent * 0.01;
			double p = Utils.Note2HzLookup(value) + _linearHzOffset;
			SetFrequencyHz(p);
		}

		double _pitch;
		public double Pitch
		{
			get { return _pitch; }
			set
			{
				if (Math.Abs(_pitch - value) < 0.0001)
					return;

				_pitch = value;
				SetFrequencyPitch(_pitch);
			}
		}

		public double Process()
		{
			Phase = (Phase + FreqRad) % 1.0;

			double[] wt1 = Waves[(int)_currWaveIdx][CurrentPartials];
			double[] wt2 = Waves[(int)_currWaveIdx + 1][CurrentPartials];
			double pos = _currWaveIdx % 1.0;

			int idx = (int)(Phase * SampleCount);
			double sample = wt1[idx] * (1 - pos) + wt2[idx] * pos;
			Output = sample;
			return Output;
		}

		public double Output;

		double _octave, _semi, _cent, _linearHzOffset;

		public double Octave
		{
			get { return _octave; }
			set { _octave = value; SetFrequencyPitch(_pitch); }
		}

		public double Semi
		{
			get { return _semi; }
			set { _semi = value; SetFrequencyPitch(_pitch); }
		}

		public double Cent
		{
			get { return _cent; }
			set { _cent = value; SetFrequencyPitch(_pitch); }
		}

		public double LinearHzOffset
		{
			get { return _linearHzOffset; }
			set { _linearHzOffset = value; SetFrequencyPitch(_pitch); }
		}

		/// <summary>
		/// returns the optimal distribution of partials per wave, make sure no partials go below
		/// the specified minimum of above the sampling frequency (causing aliasing)
		/// </summary>
		/// <param name="minimumFrequency"></param>
		/// <param name="samplerate"></param>
		/// <returns></returns>
		static List<int> GetMaxPartials(int maxPartials, double minimumFrequency, double samplerate)
		{
			double min = minimumFrequency;
			double max = samplerate;

			var partials = new List<int>();

			partials.Add(maxPartials);
			int current = maxPartials;

			for (int i = 1; i < max; i++)
			{
				var cMax = current * i;

				if (cMax >= max)
				{
					current = (int)((min - 1) / (double)i);
					if ((current + 1) * i < max)
						current++;

					partials.Add(current);
				}
			}

			return partials;
		}
	}
}
