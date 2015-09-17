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

namespace Rodent.V2
{
	public class Rodent : IAudioDevice
	{
		
		public const int P_GAIN   =	0;
		public const int P_FILTER =	1;
		public const int P_VOL    =	2;

		public const int P_RUETZ  =	3;
		public const int P_TURBO  =	4;
		public const int P_TIGHT  =	5;
		public const int P_OD     =	6;
		public const int P_ON     = 7;

		public static string[] ParameterNames = new String[] { 
			"Gain", 
			"Filter", 
			"Volume", 
			"Ruetz Mod", 
			"Turbo", 
			"Tight", 
			"Overdrive",
			"On"
		};

		// ------- Declare modules for processing ----------
		private Highpass1 Hipass1;
		private Lowpass1 Lowpass1;
		private TFGain Gain;
		private Highpass1 HipassDC;
		private SplineInterpolator Clipper;
		private SplineInterpolator Clipper2;
		private Lowpass1 Filter;
		private Highpass1 Hipass3;

		Editor e;

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public int DeviceId { get; set; }
		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;

		public Rodent()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[8];
			PortInfo = new Port[2];
			e = new Editor(this);
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Rodent.V2";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = e.Height;
			DevInfo.EditorWidth = e.Width;
			DevInfo.HasEditor = true;
			DevInfo.Name = "Rodent.V2 Beta 4";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Effect;
			DevInfo.Version = 1004;
			DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);

			PortInfo[0].Direction = PortDirection.Input;
			PortInfo[0].Name = "Mono Input";
			PortInfo[0].NumberOfChannels = 1;

			PortInfo[1].Direction = PortDirection.Output;
			PortInfo[1].Name = "Stereo Output";
			PortInfo[1].NumberOfChannels = 2;
			
			for(int i = 0; i < ParameterInfo.Length; i++)
			{
				var p = new Parameter();
				p.Display = "0.49";
				p.Index = (uint)i;
				p.Name = ParameterNames[i];
				p.Steps = 0;
				p.Value = 0.49;
				ParameterInfo[i] = p;
			}

			Hipass1 = new Highpass1((float)Samplerate);
			Lowpass1 = new Lowpass1((float)Samplerate);
			Gain = new TFGain((float)Samplerate);
			HipassDC = new Highpass1((float)Samplerate);
			/*Clipper = new LUT();
			Clipper.ReadRecord(Tables.D1N914TF.Split('\n'));
			Clipper.Table = Tables.Upsample(Clipper.Table, 100000);*/
			Clipper = new SplineInterpolator(Splines.D1N914TF);

			/*Clipper2 = new LUT();
			Clipper2.ReadRecord(Tables.LEDTF.Split('\n'));
			Clipper2.Table = Tables.Upsample(Clipper2.Table, 100000);*/
			Clipper2 = new SplineInterpolator(Splines.LEDTF);

			Filter = new Lowpass1((float)Samplerate);
			Hipass3 = new Highpass1((float)Samplerate);

			// Frequency of 0.01uF cap + 1k + 1Meg = 7.227 Hz
			Hipass1.SetParam(0, 10f + (float)Math.Round(ParameterInfo[P_TIGHT].Value) * 300f);
			// Low pass rolloff because of 1n cap, estimate
			Lowpass1.SetParam(0, 5000f);
			// This is the cap after the gain, just some value to remove DC offset
			HipassDC.SetParam(0, 10f);
			// Final cutoff frequency ~ 7.7Hz
			Hipass3.SetParam(0, 7.7f);

			SetParam(P_ON, 1.0);

			// initialize all blocks
			UpdateAll();

			e.UpdateParameters();
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void SetSampleRate(double samplerate)
		{
			Samplerate = samplerate;

			Hipass1.fs = samplerate;
			Lowpass1.fs = samplerate;
			Gain.fs = samplerate;
			HipassDC.fs = samplerate;
			Filter.fs = samplerate;
			Hipass3.fs = samplerate;

			UpdateAll();
		}

		private void UpdateAll()
		{
			Hipass1.SetParam(0, 10f + (float)Math.Round(ParameterInfo[P_TIGHT].Value) * 300f);
			Hipass1.Update();
			Lowpass1.Update();
			Gain.SetParam(TFGain.P_GAIN, Utils.ExpResponse(ParameterInfo[P_GAIN].Value));
			Gain.SetParam(TFGain.P_RUETZ, Math.Round(ParameterInfo[P_RUETZ].Value));
			Gain.Update();
			HipassDC.Update();
			double freq = (1.0 / (2.0 * Math.PI * 0.0033e-6 * (2500 + 100000 * Utils.ExpResponse(ParameterInfo[P_FILTER].Value)))); // Range: 19292.0Hz to 470.0Hz
			
			Filter.SetParam(Lowpass1.P_FREQ, freq);
			Filter.Update();
			Hipass3.Update();

			if (e != null)
				e.UpdateParameters();
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

		double[] signal;
		double[] signalClean;

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			double[] inBuffer = input[0];
			double[] outBuffer = output[0];
			double[] outBuffer2 = output[1];

			if (signal == null || signal.Length != inBuffer.Length)
			{
				signal = new double[inBuffer.Length];
				signalClean = new double[inBuffer.Length];
			}

			for (int i = 0; i < inBuffer.Length; i++)
				signal[i] = inBuffer[i];

			Hipass1.ProcessInPlace(signal);
			Utils.GainInPlace(signal, Utils.DB2gain(-3));
			Lowpass1.ProcessInPlace(signal);
			
			// Store undistorted signal, if we set mode to Overdrive then this signal is added to the distorted signal
			Array.Copy(signal, signalClean, signal.Length);

			Gain.ProcessInPlace(signal);
			// LM308 has a voltage swing of about +-4 volt, then it hard clips
			Utils.SaturateInPlace(signal, 4.0f);
			HipassDC.ProcessInPlace(signal);

			if (ParameterInfo[P_TURBO].Value >= 0.5)
			{
				Utils.SaturateInPlace(signal, 7.99f);
				Clipper2.ProcessInPlace(signal); // LEDs
				Utils.GainInPlace(signal, 0.7f);
			}
			else
			{
				Utils.SaturateInPlace(signal, 7.99f);
				Clipper.ProcessInPlace(signal); // Silicon
			}

			if (ParameterInfo[P_OD].Value > 0.5)
			{
				for (int i = 0; i < signal.Length; i++)
					signal[i] = 0.6f * signal[i] + 0.9f * signalClean[i];
			}

			Filter.ProcessInPlace(signal);
			Hipass3.ProcessInPlace(signal);
			Utils.GainInPlace(signal, Utils.ExpResponse(ParameterInfo[P_VOL].Value));

			// copy data to outBuffer
			if (ParameterInfo[P_ON].Value < 0.5)
			{
				for (int i = 0; i < signal.Length; i++)
				{
					outBuffer[i] = inBuffer[i];
					outBuffer2[i] = inBuffer[i];
				}
			}
			else
			{
				for (int i = 0; i < signal.Length; i++)
				{
					outBuffer[i] = (float)signal[i];
					outBuffer2[i] = (float)signal[i];
				}
			}
		}

		public void ProcessSample(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
			throw new NotImplementedException();
		}

		public void OpenEditor(IntPtr parentWindow)
		{
			e.UpdateParameters();
			DeviceUtilities.DockWinFormsPanel(e, parentWindow);
		}

		public void CloseEditor() { }

		public bool SendEvent(Event ev)
		{
			if(ev.Type == EventType.Parameter)
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
			catch(Exception)
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

