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

namespace SmashMaster
{
	public class SmashMasterEffect : IAudioDevice
	{
		public const int P_GAIN    = 0;
		public const int P_BASS    = 1;
		public const int P_CONTOUR = 2;
		public const int P_TREBLE  = 3;
		public const int P_HAIR    = 4;
		public const int P_VOLUME  = 5;

		public const int NUM_PARAMS = 7;

		public static string[] ParameterNames = {"Gain", "Bass", "Contour", "Treble", "Hair", "Volume"};

		// ------- Declare modules for processing ----------
		private Highpass1 Hp1;
		private Lowpass1 LpNoise;
		private Gain GainTF;
		private postGain PostGain;
		private Lowpass1 SaturateLP;
		private Highpass1 ClipperHP;
		private SplineInterpolator Clipper;
		private Tonestack Tonestack;
		private TF2 TF2;
		private Contour Contour;
		private Highpass1 OutHP;
		private Lowpass1 OutLP;

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public int DeviceId { get; set; }
		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;

		public SmashMasterEffect()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[6];
			PortInfo = new Port[2];
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Smash Master";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			DevInfo.Name = "Smash Master";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Effect;
			DevInfo.Version = 1100;
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

			Hp1 = new Highpass1((float)Samplerate);
			LpNoise = new Lowpass1((float)Samplerate);
			GainTF = new Gain(this.Samplerate);
			PostGain = new postGain(Samplerate);
			SaturateLP = new Lowpass1((float)Samplerate);
			ClipperHP = new Highpass1((float)Samplerate);
			Clipper = new SplineInterpolator(SplineInterpolator.Splines.D1N914Clipper, false);

			Tonestack = new Tonestack((float)Samplerate);
			TF2 = new TF2(Samplerate);
			Contour = new Contour(Samplerate);
			OutHP = new Highpass1((float)Samplerate);
			OutLP = new Lowpass1((float)Samplerate);

			// 16Hz, remove DC bias from input
			Hp1.SetParam(0, 16f);
			LpNoise.SetParam(0, 3500f);
			ClipperHP.SetParam(0, 10f);
			SaturateLP.SetParam(0, 15000);
			Tonestack.FenderMode = true;
			Tonestack.setComponents(0.022e-6, 0.22e-6, 0.022e-6, 1000, 5e6, 6.8e3, 25e3, 1e3, 100e3);
			OutHP.SetParam(0, 30f);

			// initialize all blocks
			UpdateAll();
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void SetSampleRate(double samplerate)
		{
			Samplerate = samplerate;

			Hp1.Fs = samplerate;
			LpNoise.Fs = samplerate;
			GainTF.Fs = samplerate;
			PostGain.Fs = samplerate;
			SaturateLP.Fs = samplerate;
			ClipperHP.Fs = samplerate;
			Tonestack.Fs = samplerate;
			TF2.Fs = samplerate;
			Contour.Fs = samplerate;
			OutHP.Fs = samplerate;
			OutLP.Fs = samplerate;

			UpdateAll();
		}

		private void UpdateAll()
		{
			GainTF.SetParam(Gain.P_GAIN, Utils.LogResponse(ParameterInfo[P_GAIN].Value));
			PostGain.SetParam(postGain.P_GAIN, Utils.ExpResponse(ParameterInfo[P_GAIN].Value));

			Tonestack.SetParam(Tonestack.P_BASS, Utils.ExpResponse(ParameterInfo[P_BASS].Value));
			Tonestack.SetParam(Tonestack.P_TREBLE, ParameterInfo[P_TREBLE].Value);
			Tonestack.SetParam(Tonestack.P_MID, 1);

			Contour.SetParam(Contour.P_CONTOUR, Utils.LogResponse(1 - ParameterInfo[P_CONTOUR].Value));

			OutLP.SetParam(0, 1600f + Utils.ExpResponse(ParameterInfo[P_HAIR].Value) * 11000f);

			Hp1.Update();
			LpNoise.Update();
			GainTF.Update();
			PostGain.Update();
			SaturateLP.Update();
			ClipperHP.Update();
			Tonestack.Update();
			TF2.Update();
			Contour.Update();
			OutHP.Update();
			OutLP.Update();
		}

		public void SetParam(int param, double value)
		{
			ParameterInfo[param].Value = value;
			ParameterInfo[param].Display = $"{value:0.00}";
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
			double[] outBuffer = output[0];
			double[] outBuffer2 = output[1];

			// Double buffer for tonestack to work on
			double[] temp = new double[inBuffer.Length];

			for (int i = 0; i < inBuffer.Length; i++)
				temp[i] = inBuffer[i];

			Hp1.ProcessInPlace(temp);
			LpNoise.ProcessInPlace(temp);
			GainTF.ProcessInPlace(temp);
			PostGain.ProcessInPlace(temp);
			Utils.GainInPlace(temp, 83);
			Utils.SaturateInPlace(temp, 4);
			SaturateLP.ProcessInPlace(temp);
			ClipperHP.ProcessInPlace(temp);
			Clipper.ProcessInPlace(temp);
			
			Tonestack.ProcessInPlace(temp);
			TF2.ProcessInPlace(temp);
			Contour.ProcessInPlace(temp);
			OutHP.ProcessInPlace(temp);
			OutLP.ProcessInPlace(temp);
			Utils.GainInPlace(temp, Utils.ExpResponse(ParameterInfo[P_VOLUME].Value));
			
			for (int i = 0; i < input[0].Length; i++)
			{
				outBuffer[i] = temp[i];
				outBuffer2[i] = temp[i];
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

