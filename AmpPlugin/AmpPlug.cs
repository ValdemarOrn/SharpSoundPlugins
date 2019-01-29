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

namespace AmpPlugin
{
	public class AmpPlug : IAudioDevice
	{
		public const int P_GAIN = 0;
		public const int P_FEEDBACK = 1;
		public const int P_BIAS = 2;
		public const int P_CUTOFF = 3;
		public const int P_VOL = 4;
		
		public const int NUM_PARAMS = 5;

		public static string[] ParameterNames = { "Gain", "Feedback", "Bias", "Cutoff", "Volume" };

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public int DeviceId { get; set; }
		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;

		private double gain, feedback, bias, cutoff;
		private GainStage gainStage;
		private Hp1 hp;
		private Lp1 lp;

		public AmpPlug()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[NUM_PARAMS];
			PortInfo = new Port[2];
			gainStage = new GainStage();
			hp = new Hp1((float)Samplerate);
			lp = new Lp1((float)Samplerate);
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - AmpPlug";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			DevInfo.Name = "AmpPlug";
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
			hp = new Hp1((float)Samplerate);
			lp = new Lp1((float)Samplerate);
			hp.CutoffHz = 400;
			lp.CutoffHz = 4000;
			gainStage.g = gain;
			gainStage.fbLevel = feedback;
			gainStage.bias = bias;
			gainStage.Update(cutoff, Samplerate);
		}

		public void SetParam(int param, double value)
		{
			ParameterInfo[param].Value = value;
			string str = $"{value:0.00}";
			if (param == P_GAIN)
			{
				gain = ValueTables.Get(value, ValueTables.Pow2) * 300;
				str = $"{gain:0.0}x";
			}
			if (param == P_FEEDBACK)
			{
				feedback = value;
			}
			if (param == P_BIAS)
			{
				bias = value;
			}
			if (param == P_CUTOFF)
			{
				cutoff = ValueTables.Get(value, ValueTables.Pow2) * 2000;
				str = $"{cutoff:0.0}hz";
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
			double[] inBuffer = input[0];
			double[] outBufferL = output[0];
			double[] outBufferR = output[1];

			// Double buffer for tonestack to work on
			double[] temp = new double[inBuffer.Length];

			for (int i = 0; i < inBuffer.Length; i++)
			{
				temp[i] = gainStage.Compute(hp.Process(inBuffer[i]) * 20);
				temp[i] = lp.Process(temp[i]);
			}


			for (int i = 0; i < input[0].Length; i++)
			{
				outBufferL[i] = temp[i] * ParameterInfo[P_VOL].Value;
				outBufferR[i] = temp[i] * ParameterInfo[P_VOL].Value;
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

