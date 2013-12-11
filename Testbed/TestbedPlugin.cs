using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;

namespace Testbed
{
	public class TestbedPlugin : IAudioDevice
	{
		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		public double Samplerate;
		TestContainer Container;

		public TestbedPlugin()
		{
			Container = TestContainer.ActiveContainer;
			Container.SetSamplerate(48000);
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[Container.Parameters.Length];
			PortInfo = new Port[2];
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Testbed";
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
			DevInfo.Name = "Testbed";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = Container.IsSynth ? DeviceType.Generator : DeviceType.Effect;
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
				p.Display = Container.GetDisplay(i);
				p.Index = (uint)i;
				if (Container.ParameterNames != null && Container.ParameterNames.Length > i)
					p.Name = Container.ParameterNames[i];
				else
					p.Name = "Param " + i;
				p.Steps = 0;
				p.Value = Container.Parameters[i];
				ParameterInfo[i] = p;
			}
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			Container.ProcessSamples(input, output, bufferSize);
		}

		public void OpenEditor(IntPtr parentWindow) { }

		public void CloseEditor() { }

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				SetParameter(ev.EventIndex, (double)ev.Data);
			}
			else if (ev.Type == EventType.Midi)
			{
				var data = (byte[])ev.Data;
				if (data[0] == 0x80)
				{
					Container.RemoveNote(data[1]);
				}
				else if (data[0] == 0x90)
				{
					if (data[2] == 0)
						Container.RemoveNote(data[1]);
					else
					{
						Container.AddNote(data[1], data[2]);
					}
				}
			}
		}

		private void SetParameter(int index, double value)
		{
			if (index >= 0 && index < ParameterInfo.Length)
			{
				ParameterInfo[index].Value = value;
				Container.Parameters[index] = value;
				Container.ParameterUpdated(index);
				ParameterInfo[index].Display = Container.GetDisplay(index);
			}
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
				Container.SetSamplerate(samplerate);
		}

		public IHostInfo HostInfo { get; set; }

	}
}

