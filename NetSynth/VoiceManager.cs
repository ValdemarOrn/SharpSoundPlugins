using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth
{
	public sealed class VoiceManager
	{
		public enum VoiceSelectionMethod
		{
			RoundRobin,
			Oldest,
			Highest,
			Lowest
		}

		public Voice[] Voices;

		public VoiceManager(int voiceCount = 1)
		{
			Voices = new Voice[voiceCount];
			for (int i = 0; i < Voices.Length; i++)
				Voices[i] = new Voice();

			VoiceSelection = VoiceSelectionMethod.RoundRobin;
		}

		public void NoteOn(int note, double gate)
		{
			int idx = GetNextVoiceIndex();
			var voice = Voices[idx];

			voice.Note = note;
			voice.Gate = gate;
		}

		public void NoteOff(int note)
		{
			int idx = -1;
			for (int i = 0; i < Voices.Length; i++)
				if (Voices[i].Note == note)
					idx = i;

			if (idx == -1)
				return;

			Voices[idx].Gate = 0;
		}

		public VoiceSelectionMethod VoiceSelection;

		int lastVoiceIndex;
		int GetNextVoiceIndex()
		{
			if(VoiceSelection == VoiceSelectionMethod.RoundRobin)
			{
				lastVoiceIndex = (lastVoiceIndex + 1) % Voices.Length;
				return lastVoiceIndex;
			}

			return lastVoiceIndex;
		}
	}
}
