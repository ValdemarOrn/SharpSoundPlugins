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

namespace MrFuzz
{
	public class MrFuzzModule : IAudioDevice
	{

		public const int P_GAIN = 0;
		public const int P_FILTER = 1;
		public const int P_VOLUME = 2;

		public static string[] ParameterNames = new String[] { 
			"Gain", 
			"Filter",
			"Volume"
		};

		// ------- Declare modules for processing ----------

		Highpass1 HpInput;
		TFStage TFStage;
		LUT DiodeStage;
		Lowpass1 LowpassFilter;
		Highpass1 HpOutput;

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public int DeviceId { get; set; }
		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;

		public MrFuzzModule()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[3];
			PortInfo = new Port[2];
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Mr. Fuzz";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.HasEditor = false;
			DevInfo.Name = "Mr. Fuzz";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Effect;
			DevInfo.Version = 1100;
			DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);

			PortInfo[0].Direction = PortDirection.Input;
			PortInfo[0].Name = "Mono Input";
			PortInfo[0].NumberOfChannels = 1;

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

			HpInput = new Highpass1((float)Samplerate);
			TFStage = new TFStage((float)Samplerate);
			DiodeStage = new LUT();
			DiodeStage.ReadRecord(Table.DiodeResponse);
			DiodeStage.Table = Utils.MovingAve(DiodeStage.Table, (int)(DiodeStage.Table.Length / 200.0));
			DiodeStage.Table = Table.Upsample(DiodeStage.Table, 100000);
			LowpassFilter = new Lowpass1((float)Samplerate);
			HpOutput = new Highpass1((float)Samplerate);

			// initialize all blocks
			UpdateAll();
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void SetSampleRate(double samplerate)
		{
			Samplerate = samplerate;
			HpInput.Fs = samplerate;
			TFStage.Fs = samplerate;
			LowpassFilter.Fs = samplerate;
			HpOutput.Fs = samplerate;
			UpdateAll();
		}

		private void UpdateAll()
		{
			HpInput.SetParam(Highpass1.P_FREQ, 15);
			TFStage.Update();
			var lowpassFreq = 1592 + Utils.ExpResponse(ParameterInfo[P_FILTER].Value) * 8000;
			LowpassFilter.SetParam(Lowpass1.P_FREQ, lowpassFreq);
			HpOutput.SetParam(Highpass1.P_FREQ, 30);
		}

		public void SetParam(int param, double value)
		{
			ParameterInfo[param].Value = value;
			ParameterInfo[param].Display = String.Format("{0:0.00}", value);
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

			var inputGain = 0.4; // make input signal strength closer to that of a guitar pickup voltage
			var gain = Utils.ExpResponse(ParameterInfo[P_GAIN].Value) * inputGain;
			var volume = Utils.ExpResponse(ParameterInfo[P_VOLUME].Value);
			var GainInv = 2.2 / 1000.0;

			HpInput.ProcessInPlace(inBuffer);
			Utils.GainInPlace(inBuffer, gain);
			TFStage.ProcessInPlace(inBuffer);
			var diodeOutput = DiodeStage.GetValues(inBuffer);
			LowpassFilter.ProcessInPlace(diodeOutput);

			// sum the diode response to the clean component with gain adjustment for clean
			for(int i = 0; i < diodeOutput.Length; i++)
				diodeOutput[i] = diodeOutput[i] + GainInv * inBuffer[i];

			HpOutput.ProcessInPlace(diodeOutput);
			Utils.GainInPlace(diodeOutput, volume);

			for (int i = 0; i < diodeOutput.Length; i++)
			{
				outBufferL[i] = diodeOutput[i];
				outBufferR[i] = diodeOutput[i];
			}
		}

		public void ProcessSample(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
			throw new NotImplementedException();
		}

		public void OpenEditor(IntPtr parentWindow)
		{
			//e.UpdateParameters();
			//Interop.DockWinFormsPanel(e, parentWindow);
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
				System.Windows.Forms.MessageBox.Show("Unable to load preset");
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

