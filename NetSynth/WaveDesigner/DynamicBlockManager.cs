using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveDesigner
{
	public class DynamicBlockManager
	{
		public List<DynamicBlock> Blocks;

		public int Length;
		public int Partials;

		int _waveIndex;
		public int WaveIndex
		{
			get { return _waveIndex; }
			set
			{
				_waveIndex = value;
				if (Blocks == null)
					return;

				foreach (var block in Blocks)
				{
					block.Manager = this; // this is used when deserializing to make the inverse connection (from block to manager)
					block.UpdateValues();
				}
			}
		}

		public int WaveCount;

		public DynamicBlockManager()
		{
			Blocks = new List<DynamicBlock>();
		}

		public void Process(int waveIndex)
		{
			for (int i = 0; i < Blocks.Count; i++)
			{
				var block = Blocks[i];

				block.Length = Length;
				block.Partials = Partials;

				if(i == 0)
				{
					block.InputPhase = null;
					block.InputSignal = null;
				}
				else
				{
					var previous = Blocks[i - 1];
					if(block.IsFrequencyDomain)
					{
						var dft = previous.GetFreqDomainSignal();
						block.InputSignal = dft.Item1;
						block.InputPhase = dft.Item2;
					}
					else
					{
						var signal = previous.GetTimeDomainSignal();
						block.InputSignal = signal;
						block.InputPhase = null;
					}
				}

				try
				{
					block.Process(waveIndex);
					block.ErrorText = "";
				}
				catch (Exception e)
				{
					block.OutputPhase = null;
					block.OutputSignal = null;
					block.ErrorText = e.Message;

					if (e.InnerException == null)
						return;

					block.ErrorText += "\n" + e.InnerException.Message;

					if (e.InnerException.InnerException == null)
						return;

					block.ErrorText += "\n" + e.InnerException.InnerException.Message;					
				}
			}
		}

		public void MoveUp(DynamicBlock instance)
		{
			var index = Blocks.IndexOf(instance);
			if (index <= 0)
				return;

			var prev = Blocks[index - 1];
			Blocks[index - 1] = instance;
			Blocks[index] = prev;
		}

		public void MoveDown(DynamicBlock instance)
		{
			var index = Blocks.IndexOf(instance);
			if (index < 0 || index == Blocks.Count - 1)
				return;

			var next = Blocks[index + 1];
			Blocks[index + 1] = instance;
			Blocks[index] = next;
		}

		public string Serialize()
		{ 
			var output = Serializer.SerializeToXML(this);
			//var man2 = (DynamicBlockManager)Serializer.DeserializeToXML(output, typeof(DynamicBlockManager));
			return output;
		}

	}
}
