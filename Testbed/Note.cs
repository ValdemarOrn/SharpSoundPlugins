using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testbed
{
	public struct Note
	{
		public int Pitch;
		public int Velocity;

		public Note(int pitch, int velocity)
		{
			Pitch = pitch;
			Velocity = velocity;
		}
	}
}
