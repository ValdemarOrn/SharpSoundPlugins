using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testbed
{
	public abstract class TestContainer
	{
		public static TestContainer ActiveContainer = new Reverb();

		public bool IsSynth;
		public string[] ParameterNames;
		public double[] Parameters;
		public double[][] Inputs;
		public double[][] Outputs;
		protected List<Note> Notes;

		public double Samplerate;

		public TestContainer() 
		{
			Notes = new List<Note>();
		}

		public void ProcessSamples(double[][] input, double[][] output, uint bufferSize)
		{
			Inputs = input;
			Outputs = output;
			Process();
		}

		public void AddNote(int pitch, int velocity)
		{
			for(int i = 0; i < Notes.Count; i++)
			{
				if (Notes[i].Pitch == pitch)
				{
					Notes[i] = new Note(pitch, velocity);
					break;
				}
			}

			Notes.Add(new Note(pitch, velocity));
		}

		internal void RemoveNote(int pitch)
		{
			for (int i = 0; i < Notes.Count; i++)
			{
				if (Notes[i].Pitch == pitch)
				{
					Notes.RemoveAt(i);
					break;
				}
			}
		}

		public abstract void Process();

		public virtual void SetSamplerate(double rate)
		{
			Samplerate = rate;
		}

		public virtual string GetDisplay(int i)
		{
			return String.Format("{0:0.00}", Parameters[i]);
		}

		public virtual void ParameterUpdated(int i)
		{
		}
		
	}
}
