using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;
using AudioLib.Modules;

namespace BiquadModule
{
	public class BiquadPlugin : IAudioDevice
	{
		public const int P_TYPE = 0;
		public const int P_FREQ = 1;
		public const int P_SLOPE = 2;
		public const int P_Q = 3;
		public const int P_GAIN = 4;

		public static string[] ParameterNames = new String[] { 
			"Filter Type", 
			"Frequency", 
			"Shelf Slope",
			"Q Factor",
			"Gain"
		};

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public int DeviceId { get; set; }
		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;

		public int FilterType;
		public double Frequency;
		public double Slope;
		public double Q;
		public double GainDB;

		public Biquad BiquadL;
		public Biquad BiquadR;

		public BiquadPlugin()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[5];
			PortInfo = new Port[2];
			BiquadL = new Biquad(Biquad.FilterType.LowPass, 48000);
			BiquadR = new Biquad(Biquad.FilterType.LowPass, 48000);
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Biquad";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			DevInfo.Name = "Biquad Filter";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Effect;
			DevInfo.Version = 1000;
			DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);

			PortInfo[0].Direction = PortDirection.Input;
			PortInfo[0].Name = "Stereo Input";
			PortInfo[0].NumberOfChannels = 2;

			PortInfo[1].Direction = PortDirection.Output;
			PortInfo[1].Name = "Stereo Output";
			PortInfo[1].NumberOfChannels = 2;

			for (int i = 0; i < ParameterInfo.Length; i++)
			{
				var p = new Parameter();
				p.Display = "0.0";
				p.Index = (uint)i;
				p.Name = ParameterNames[i];
				p.Steps = 0;
				p.Value = 0.0;
				ParameterInfo[i] = p;
			}

			SetParameter(P_TYPE, 0.0);
			SetParameter(P_FREQ, 0.5);
			SetParameter(P_SLOPE, 0.5);
			SetParameter(P_Q, 0.5);
			SetParameter(P_GAIN, 0.5);
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			double[] inLeft = input[0];
			double[] inRight = input[1];

			double[] outLeft = output[0];
			double[] outRight = output[1];

			// copy data to outBuffer
			for (int i = 0; i < bufferSize; i++)
			{
				outLeft[i] = BiquadL.Process(inLeft[i]);
				outRight[i] = BiquadR.Process(inRight[i]);
			}
		}

		public void ProcessSample(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
			throw new NotImplementedException();
		}

		public void OpenEditor(IntPtr parentWindow) { }

		public void CloseEditor() { }

		public bool SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				SetParameter(ev.EventIndex, (double)ev.Data);
				return true;
			}

			return false;
		}

		private void SetParameter(int index, double value)
		{


			if (index == P_TYPE)
			{
				FilterType = (int)(value * 6.99);
				BiquadL.Type = (Biquad.FilterType)FilterType;
				BiquadR.Type = (Biquad.FilterType)FilterType;

				ParameterInfo[index].Display = BiquadL.Type.ToString();
			}
			else if (index == P_FREQ)
			{
				Frequency = ValueTables.Get(value, ValueTables.Response3Dec) * 22000;
				BiquadL.Frequency = Frequency;
				BiquadR.Frequency = Frequency;
				ParameterInfo[index].Display = String.Format("{0:0.00}", Frequency);
			}
			else if (index == P_SLOPE)
			{
				Slope = value;
				BiquadL.Slope = Slope;
				BiquadR.Slope = Slope;
				ParameterInfo[index].Display = String.Format("{0:0.00}", Slope);
			}
			else if (index == P_Q)
			{
				if (value < 0.5)
					Q = 0.1 + 0.9 * 2 * value;
				else
					Q = 1 + (value - 0.5) * 12;

				BiquadL.Q = Q;
				BiquadR.Q = Q;
				ParameterInfo[index].Display = String.Format("{0:0.00}", Q);
			}
			else if (index == P_GAIN)
			{
				GainDB = (2 * value - 1) * 20;
				BiquadL.GainDB = GainDB;
				BiquadR.GainDB = GainDB;
				ParameterInfo[index].Display = String.Format("{0:0.0}", GainDB);
			}

			ParameterInfo[index].Value = value;

			BiquadL.Update();
			BiquadR.Update();

		}

		System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

		string Serialize()
		{
			string output = "";
			foreach (var p in ParameterInfo)
				output += p.Value.ToString(culture) + ", ";

			return output;
		}

		List<double> Deserialize(string input)
		{
			var items = input.Split(',').Where(x => x != null && x != "").Select(x => Convert.ToDouble(x.Trim(), culture)).ToList();
			return items;
		}

		public void SetProgramData(Program program, int index)
		{
			string data = Encoding.UTF8.GetString(program.Data);
			var values = Deserialize(data);
			if (values.Count != ParameterInfo.Length)
				throw new Exception("Illegal program data. Number of parameters does not match");

			for (int i = 0; i < ParameterInfo.Length; i++)
				ParameterInfo[i].Value = values[i];
		}

		public Program GetProgramData(int index)
		{
			var output = new Program();
			output.Data = Encoding.UTF8.GetBytes(Serialize());
			output.Name = "Program";
			return output;
		}

		public void HostChanged()
		{
			var samplerate = HostInfo.SampleRate;
			if (samplerate != this.Samplerate)
				this.Samplerate = samplerate;
		}

		public IHostInfo HostInfo { get; set; }

	}
}

