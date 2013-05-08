using AudioLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveDesigner
{
	public class DynamicBlock
	{
		[System.Xml.Serialization.XmlIgnore]
		public DynamicBlockManager Manager;

		public bool IsFrequencyDomain;

		public List<DynamicBlockParameter> Parameters;

		[System.Xml.Serialization.XmlIgnore]
		public int Length;
		[System.Xml.Serialization.XmlIgnore]
		public int Partials;

		[System.Xml.Serialization.XmlIgnore]
		public double[] InputSignal;
		[System.Xml.Serialization.XmlIgnore]
		public double[] InputPhase;

		[System.Xml.Serialization.XmlIgnore]
		public double[] OutputSignal;
		[System.Xml.Serialization.XmlIgnore]
		public double[] OutputPhase;

		public string SetupCode;
		public string ProcessCode;

		[System.Xml.Serialization.XmlIgnore]
		public string ErrorText;

		public DynamicBlock()
		{
			Parameters = new List<DynamicBlockParameter>();
		}

		public DynamicBlock(DynamicBlockManager manager) : this()
		{
			Manager = manager;
		}

		[System.Xml.Serialization.XmlIgnore]
		public int Index
		{
			get
			{
				if (Manager == null)
					return -1;

				return Manager.Blocks.IndexOf(this);
			}
		}

		public void Process(int waveIndex)
		{
			double[][] result = null;

			if (IsFrequencyDomain)
			{
				result = DynamicTable.Evaluate(Partials, waveIndex, InputSignal, InputPhase, Parameters.Select(x => x.ValueStart).ToArray(), SetupCode, ProcessCode);
				OutputSignal = result[0];
				OutputPhase = result[1];
			}
			else
			{
				result = DynamicTable.Evaluate(Length, waveIndex, InputSignal, null, Parameters.Select(x => x.ValueStart).ToArray(), SetupCode, ProcessCode);
				OutputSignal = result[0];
				OutputPhase = null;
			}
		}

		public double[] GetTimeDomainSignal()
		{
			if(!IsFrequencyDomain)
				return OutputSignal;

			if (OutputSignal == null || OutputPhase == null)
				return null;

			var signal = SimpleDFT.IDFT(new Pair<double[], double[]>(OutputSignal, OutputPhase), Length, Partials);
			return signal;
		}

		public Pair<double[], double[]> GetFreqDomainSignal()
		{
			if (IsFrequencyDomain)
			{
				if (OutputSignal == null || OutputPhase == null)
					return null;

				return new Pair<double[], double[]>(OutputSignal, OutputPhase);
			}

			if (OutputSignal == null)
				return new Pair<double[], double[]>(null, null);

			var dft = SimpleDFT.DFT(OutputSignal, Partials);
			return dft;
		}


		/// <summary>
		/// Updates parameter values after changing the wave index in manager
		/// </summary>
		public void UpdateValues()
		{
			/*foreach(var param in Parameters)
			{
				param.Value = CurrentValues[param.Index];
			}*/
		}

		public void SetParamCount(int count)
		{
			while (Parameters.Count < count)
			{
				var newParam = new DynamicBlockParameter();
				newParam.ValueStart = 0.5;
				newParam.ValueStop = 0.5;
				newParam.Min = 0;
				newParam.Max = 1;
				newParam.Index = Parameters.Count;
				newParam.Name = "Parameter " + (Parameters.Count + 1);
				Parameters.Add(newParam);

			}

			while (Parameters.Count > count)
			{
				Parameters.RemoveAt(Parameters.Count - 1);
			}
		}
	}
}
