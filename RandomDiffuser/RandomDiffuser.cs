using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using AudioLib.TF;
using AudioLib;
using SharpSoundDevice;
using AudioLib.Modules;
using AudioLib.SplineLut;

namespace RandomDiffuser
{
	public class RandomDiffuserPlug : IAudioDevice
	{
		public const int P_MIX = 0;
		public const int P_COUNT = 1;
		public const int P_2 = 2;
		public const int P_3 = 3;
		public const int P_4 = 4;
		
		public const int NUM_PARAMS = 5;

		public static string[] ParameterNames = { "Mix", "Grain Count", "Param2", "Param3", "Param4" };

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public int DeviceId { get; set; }
		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;
		private double[][] InBuffer;
		private int InBufferIdx;
		private Grain[] Grains;

		private double mix;
		private int grainCount;

		public RandomDiffuserPlug()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[NUM_PARAMS];
			PortInfo = new Port[2];
			InBuffer = new[] { new double[200000], new double[200000] };
			grainCount = 1;
			Grains = Enumerable.Range(0, 100).Select(x => new Grain()).ToArray();
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Random Diffuser";
			DevInfo.Name = "RandomDiffuser";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
			DevInfo.Name = "RandomDiffuser - DEV";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			
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
				p.Display = "0.5";
				p.Index = (uint)i;
				p.Name = ParameterNames[i];
				p.Steps = 0;
				p.Value = 0.5;
				ParameterInfo[i] = p;
			}

			UpdateAll();
		}

		public void DisposeDevice() { }
		public void Start() { }
		public void Stop() { }

		public void SetSampleRate(double samplerate)
		{
			Samplerate = samplerate;

			UpdateAll();
		}

		private void UpdateAll()
		{

		}

		public void SetParam(int param, double value)
		{
			ParameterInfo[param].Value = value;
			string str = $"{value:0.00}";
			if (param == P_MIX)
			{
				mix = value;
				str = $"{mix:0.0}";
			}
			else if (param == P_COUNT)
			{
				grainCount = 1 + (int)(value * 99.99);
				str = $"{grainCount:0}";
			}

			ParameterInfo[param].Display = str;
			UpdateAll();
		}

		public void SetCurrentProgram(int program)
		{
			CurrentProgram = program;
			UpdateAll();
		}

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			var inv = 1.0 / Math.Sqrt(grainCount);

			for (int i = 0; i < input[0].Length; i++)
			{
				InBuffer[0][InBufferIdx] = input[0][i];
				InBuffer[1][InBufferIdx] = input[1][i];

				for (int g = 0; g < grainCount; g++)
				{
					var grain = Grains[g];
					grain.Mix = mix;
					grain.Process(InBuffer, InBufferIdx, output, i);
				}
								
				output[0][i] *= inv;
				output[1][i] *= inv;
				output[0][i] += (1 - mix) * input[0][i];
				output[1][i] += (1 - mix) * input[1][i];
				InBufferIdx = (InBufferIdx + 1) % InBuffer[0].Length;
			}
		}

		public void ProcessSample(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
			throw new NotImplementedException();
		}

		public void OpenEditor(IntPtr parentWindow)
		{
		}

		public void CloseEditor() { }

		public bool SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				SetParam((int)ev.EventIndex, (double)ev.Data);
				return true;
			}

			return false;
		}

		public void SetProgramData(Program program, int index)
		{
			try
			{
				DeviceUtilities.DeserializeParameters(ParameterInfo, program.Data);
				UpdateAll();
			}
			catch (Exception)
			{

			}
		}

		public Program GetProgramData(int index)
		{
			var output = new Program();
			output.Data = DeviceUtilities.SerializeParameters(ParameterInfo);
			output.Name = "";
			return output;
		}

		public void HostChanged()
		{
			var samplerate = HostInfo.SampleRate;
			if (samplerate != this.Samplerate)
				SetSampleRate(samplerate);
		}

		public IHostInfo HostInfo { get; set; }

	}
}

