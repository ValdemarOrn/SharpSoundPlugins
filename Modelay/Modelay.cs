using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;

namespace ModelayPlugin
{
	public class Modelay : IAudioDevice
	{
		public const int P_DELAY = 0;
		public const int P_MODDEPTH = 1;
		public const int P_RATE = 2;

		public static string[] ParameterNames = new String[] { 
			"Delay", 
			"Depth", 
			"Rate"
		};

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;

		public double DelayMs;
		public double Depth;
		public double Rate;

		public Modelay()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[3];
			PortInfo = new Port[2];
			BufferLeft = new CircularBuffer(48000);
			BufferRight = new CircularBuffer(48000);
			Random = new Random();
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Modelay";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			DevInfo.Name = "Modelay - Random Modulation Stereo Delay";
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

			SetParameter(P_DELAY, 0.5);
			SetParameter(P_MODDEPTH, 0.5);
			SetParameter(P_RATE, 0.5);
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		CircularBuffer BufferLeft;
		CircularBuffer BufferRight;

		Random Random;

		double RandWalkL;
		double RandWalkR;

		double ModLeft;
		double ModRight;

		int iterator;

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			double[] inLeft = input[0];
			double[] inRight = input[1];

			double[] outLeft = output[0];
			double[] outRight = output[1];

			for (int i = 0; i < bufferSize; i++)
			{
				iterator++;

				if (iterator > (1 - Rate) * 48000)
				{
					iterator = 0;
					var dx = 0.1;

					var r = Random.NextDouble();
					if (r <= 0.5)
					{
						var effect = (1 + RandWalkL) * 0.25 + 0.5;
						RandWalkL = RandWalkL - dx * effect;
					}
					else
					{
						var effect = (RandWalkL - 1) * (-0.25) + 0.5;
						RandWalkL = RandWalkL + dx * effect;
					}

					r = Random.NextDouble();
					if (r <= 0.5)
					{
						var effect = (1 + RandWalkR) * 0.25 + 0.5;
						RandWalkR = RandWalkR - dx * effect;
					}
					else
					{
						var effect = (RandWalkR - 1) * (-0.25) + 0.5;
						RandWalkR = RandWalkR + dx * effect;
					}
				}

				var f = Depth * 0.05;
				ModLeft = f * RandWalkL + (1 - f) * ModLeft;
				ModRight = f * RandWalkR + (1 - f) * ModRight;

				//BufferLeft.Write(inLeft[i]);
				//BufferRight.Write(inRight[i]);

				//double L = BufferLeft.Read(-DelayLeftSamples);
				//double R = BufferRight.Read(-DelayRightSamples);

				outLeft[i] = ModLeft;
				outRight[i] = ModRight;
			}
		}

		public void OpenEditor(IntPtr parentWindow) { }

		public void CloseEditor() { }

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
				SetParameter(ev.EventIndex, (double)ev.Data);
		}

		private void SetParameter(int index, double value)
		{
			if (index == P_DELAY)
			{
				DelayMs = (2.0 * value - 1.0) * 20;
				ParameterInfo[index].Display = String.Format("{0:0.00}", DelayMs);
			}
			else if (index == P_MODDEPTH)
			{
				Depth = value;
				ParameterInfo[index].Display = String.Format("{0:0.00}", Depth);
			}
			else if (index == P_RATE)
			{
				Rate = value;
				ParameterInfo[index].Display = String.Format("{0:0.00}", Rate);
			}

			ParameterInfo[index].Value = value;

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

