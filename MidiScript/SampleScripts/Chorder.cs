using AudioLib.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiScript.SampleScripts
{
	public class Chorder : IMidiPlugin
	{
		public Action<byte[]> Send { get; set; }

		public void Process(byte[] data)
		{
			var midi = new MidiHelper(data);
			
			if (midi.MsgType == MessageType.NoteOn)
			{
				Send(midi.Data);
				midi.NoteNumber += 7;
				Send(midi.Data);
            }
			else if (midi.MsgType == MessageType.NoteOff)
			{
				Send(midi.Data);
				midi.NoteNumber += 7;
				Send(midi.Data);
			}
		}

		public void SampleProcess(int samplerate, int bufferSize)
		{
			
		}
	}
}
