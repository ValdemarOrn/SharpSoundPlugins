using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiScript
{
	public interface IMidiPlugin
	{
		Action<byte[]> Send { get; set; }
		void Process(byte[] message);
		void SampleProcess(int samplerate, int bufferSize);
	}
}
