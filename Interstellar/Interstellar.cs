using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;
using AudioLib.Modules;
using System.Net.Sockets;
using System.Net;

namespace Interstellar
{
	public class InterstellarPlugin : IAudioDevice
	{
		public const int P_Volume = 0;


		public static string[] ParameterNames = new String[] { 
			"Volume"
		};

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;

		public InterstellarPlugin()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[1];
			PortInfo = new Port[2];
		}

		public void InitializeDevice()
		{
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.DeviceID = "Analog Window - Interstellar IPC Plugin";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			DevInfo.Name = "Interstellar IPC Plugin";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Effect;
			DevInfo.Version = 1000;
			DevInfo.VstId = 456812594;

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

			SetParameter(P_Volume, 0.5);


			string prc = @"C:\Src\_Tree\Audio\OscAP\OscAPGain\bin\x86\Debug\OscAPGain.exe";
			Driver = new OscAP.HostDriver(prc);
		}


		OscAP.HostDriver Driver;

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			double[] inLeft = input[0];
			double[] inRight = input[1];

			double[] outLeft = output[0];
			double[] outRight = output[1];

			float[] inLeftFloat = new float[inLeft.Length];
			for (int i = 0; i < inLeft.Length; i++)
				inLeftFloat[i] = (float)inLeft[i];

			var recv = Driver.ProcessSamples(1, inLeft.Length, inLeftFloat);

			// copy data to outBuffer
			for (int i = 0; i < bufferSize; i++)
			{
				outLeft[i] = recv[i] * ParameterInfo[P_Volume].Value;
				outRight[i] = recv[i] * ParameterInfo[P_Volume].Value;
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
			ParameterInfo[index].Display = String.Format("{0:0.00}", value);
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

		public HostInfo HostInfo { get; set; }

	}
}

