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
		public const int P_PREDELAY = 2;
		public const int P_DELAY = 3;
		public const int P_DELAY_SCATTER = 4;
		public const int P_PITCH_SCATTER = 5;
		public const int P_SEED = 6;

		public const int NUM_PARAMS = 7;

		public static string[] ParameterNames = {
			"Mix",
			"Grain Count",
			"Pre-Delay",
			"Delay",
			"Delay Scatter",
			"Pitch Scatter",
			"Seed"
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
		private GrainDelay GrainDelay;
		
		public RandomDiffuserPlug()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[NUM_PARAMS];
			PortInfo = new Port[2];
			GrainDelay = new GrainDelay();
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
		}

		public void DisposeDevice() { }
		public void Start() { }
		public void Stop() { }

		public void SetSampleRate(double samplerate)
		{
			Samplerate = samplerate;
			foreach (var p in ParameterInfo)
				SetParam((int)p.Index, p.Value);
		}

		public void SetParam(int param, double value)
		{
			ParameterInfo[param].Value = value;
			string str = $"{value:0.00}";
			if (param == P_MIX)
			{
				GrainDelay.Config.Mix = value;
				str = $"{GrainDelay.Config.Mix*100:0.0}%";
			}
			else if (param == P_COUNT)
			{
				GrainDelay.Config.ActiveGrains = 1 + (int)(value * (GrainDelay.MaxGrains - 0.01));
				str = $"{GrainDelay.Config.ActiveGrains:0}";
			}
			else if (param == P_PREDELAY)
			{
				var seconds = ValueTables.Get(value, ValueTables.Response2Dec) * 0.2;
				GrainDelay.Config.PreDelay = seconds * Samplerate;
				str = $"{seconds*1000:0}ms";
			}
			else if (param == P_DELAY)
			{
				var seconds = ValueTables.Get(value, ValueTables.Response2Dec) * 0.5;
				GrainDelay.Config.Delay = seconds * Samplerate;
				str = $"{seconds * 1000:0}ms";
			}
			else if (param == P_DELAY_SCATTER)
			{
				var scatter = 1 + value * 9;
				GrainDelay.Config.DelayScatter = scatter;
				str = $"{scatter:0.00}x";
			}
			else if (param == P_PITCH_SCATTER)
			{
				var scatter = value;
				GrainDelay.Config.PitchScatter = scatter;
				str = $"{scatter:0.00}x";
			}
			else if (param == P_SEED)
			{
				var seed = (int)(value * 1000);
				GrainDelay.Config.Seed = seed;
				str = $"{seed:0}";
			}

			GrainDelay.Config.Length = 0.2 * Samplerate;
			GrainDelay.Config.LengthScatter = 3.0;
			GrainDelay.Configure();
			ParameterInfo[param].Display = str;
		}

		public void SetCurrentProgram(int program)
		{
			CurrentProgram = program;
		}

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			for (int i = 0; i < input[0].Length; i++)
			{
				GrainDelay.Process(input, i, output, i);
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

