using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveDesigner
{
	public class DynamicBlockParameter
	{
		public int Index;
		public string Name;

		public double ValueStart;
		public double ValueStop;

		public double Min;
		public double Max;
		public bool IsInteger;
	}
}
