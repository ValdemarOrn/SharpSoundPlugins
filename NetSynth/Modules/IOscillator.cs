using System;
namespace NetSynth.Modules
{
	public interface IOscillator
	{
		double Samplerate { get; set; }
		double Pitch { get; set; }

		double Octave { get; set; }
		double Semi { get; set; }
		double Cent { get; set; }
		double LinearHzOffset { get; set; }

		double Process();
	}
}
