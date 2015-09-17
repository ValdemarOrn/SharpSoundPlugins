using AudioLib.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiScript.SampleScripts
{
	public class Incrementor : IMidiPlugin
	{
		public int CcNumber = 21;
		public int Value;

		public Action<byte[]> Send { get; set; }

		public void Process(byte[] message)
		{
			var midi = new MidiHelper(message);
			if (midi.MsgType == MessageType.ControlChange)
			{
				if (midi.ControlNumber == 90 && midi.ControlValue == 127)
					Value--;
				if (midi.ControlNumber == 91 && midi.ControlValue == 127)
					Value++;

				if (Value < 0)
					Value = 127;
				if (Value > 127)
					Value = 0;

				if (midi.ControlValue == 127)
					Send(new MidiHelper(MessageType.ControlChange, 0, CcNumber, Value).Data);
			}
		}

		public void SampleProcess(int samplerate, int bufferSize)
		{
			
		}
	}
}
