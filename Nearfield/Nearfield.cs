using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;

namespace Nearfield
{
	public class Nearfield : IAudioDevice
	{
		public const int P_WIDTH = 0;
		public const int P_DELAYLEFT = 1;
		public const int P_DELAYRIGHT = 2;
		public const int P_VOLUMELEFT = 3;
		public const int P_VOLUMERIGHT = 4;

		public static string[] ParameterNames = new String[] { 
			"Width", 
			"Delay Left", 
			"Delay Right",
			"Volume Left",
			"Volume Right"
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

		public int DelayLeftSamples;
		public int DelayRightSamples;
		public double StereoWidth;
		public double VolumeLeft;
		public double VolumeRight;

		public Nearfield()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[5];
			PortInfo = new Port[2];
			BufferLeft = new CircularBuffer(5000);
			BufferRight = new CircularBuffer(5000);
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Nearfield";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			DevInfo.Name = "Nearfield - Stereo Manipulator";
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

			SetParameter(P_WIDTH, 0.5);
			SetParameter(P_DELAYLEFT, 0.0);
			SetParameter(P_DELAYRIGHT, 0.0);
			SetParameter(P_VOLUMELEFT, 0.5);
			SetParameter(P_VOLUMERIGHT, 0.5);
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		CircularBuffer BufferLeft;
		CircularBuffer BufferRight;

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			double[] inLeft = input[0];
			double[] inRight = input[1];

			double[] outLeft = output[0];
			double[] outRight = output[1];

			// copy data to outBuffer
			for (int i = 0; i < bufferSize; i++)
			{
				BufferLeft.Write(inLeft[i]);
				BufferRight.Write(inRight[i]);

				double L = BufferLeft.Read(-DelayLeftSamples);
				double R = BufferRight.Read(-DelayRightSamples);

				outLeft[i] = (L - StereoWidth * R) * VolumeLeft;
				outRight[i] = (R - StereoWidth * L) * VolumeRight;
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
			if (index == P_WIDTH)
			{
				StereoWidth = value * 2 - 1.0;
				ParameterInfo[index].Display = String.Format("{0:0.00}", StereoWidth);
			}
			else if (index == P_DELAYLEFT)
			{
				DelayLeftSamples = (int)(value * 1025);
				ParameterInfo[index].Display = String.Format("{0:0.00}", DelayLeftSamples);
			}
			else if (index == P_DELAYRIGHT)
			{
				DelayRightSamples = (int)(value * 1025);
				ParameterInfo[index].Display = String.Format("{0:0.00}", DelayRightSamples);
			}
			else if (index == P_VOLUMELEFT)
			{
				VolumeLeft = value * 2;
				ParameterInfo[index].Display = String.Format("{0:0.00}", VolumeLeft);
			}
			else if (index == P_VOLUMERIGHT)
			{
				VolumeRight = value * 2;
				ParameterInfo[index].Display = String.Format("{0:0.00}", VolumeRight);
			}

			ParameterInfo[index].Value = value;
		}

		public void SetProgramData(Program program, int index)
		{
			try
			{
				DeviceUtilities.DeserializeParameters(ParameterInfo, program.Data);
				for (int i = 0; i < ParameterInfo.Length; i++)
					SetParameter(i, ParameterInfo[i].Value);
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
				this.Samplerate = samplerate;
		}

		public IHostInfo HostInfo { get; set; }

	}
}

