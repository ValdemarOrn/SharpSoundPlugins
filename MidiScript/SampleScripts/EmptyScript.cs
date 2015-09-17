using AudioLib.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiScript.SampleScripts
{
	public class EmptyScript : IMidiPlugin
	{
		public Action<byte[]> Send { get; set; }

		public void Process(byte[] data)
		{
			var midi = new MidiHelper(data);
			Send(midi.Data);
		}

		public void SampleProcess(int samplerate, int bufferSize)
		{
		}
	}
}