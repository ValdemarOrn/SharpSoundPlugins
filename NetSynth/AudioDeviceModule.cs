using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;
using NetSynth.Modules;

namespace NetSynth
{
	public sealed class AudioDeviceModule : IAudioDevice
	{
		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- ----------------------- ---------------

		public SynthController Controller;

		public double Samplerate;

		public AudioDeviceModule()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[1];
			PortInfo = new Port[1];
			Controller = new SynthController(this);
		}

		public void InitializeDevice()
		{
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.DeviceID = "Analog Window - NetSynth";
			DevInfo.EditorHeight = (Controller.View != null) ? (int)Controller.View.Height : 400;
			DevInfo.EditorWidth = (Controller.View != null) ? (int)Controller.View.Width : 800;
			DevInfo.HasEditor = true;
			DevInfo.Name = "NetSynth Software Synthesizer";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Generator;
			DevInfo.Version = 1000;
			DevInfo.VstId = 1;

			//PortInfo[0].Direction = PortDirection.Input;
			//PortInfo[0].Name = "Stereo Input";
			//PortInfo[0].NumberOfChannels = 2;

			PortInfo[0].Direction = PortDirection.Output;
			PortInfo[0].Name = "Stereo Output";
			PortInfo[0].NumberOfChannels = 2;

			for (int i = 0; i < ParameterInfo.Length; i++)
			{
				var p = new Parameter();
				p.Display = "0.0";
				p.Index = (uint)i;
				p.Name = "Parameter";
				p.Steps = 0;
				p.Value = 0.0;
				ParameterInfo[i] = p;
				//UpdateParameterDisplay(i, 0.5);
			}
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		Random r = new Random();

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			double[] outLeft = output[0];
			double[] outRight = output[1];

			var man = Controller.Manager;

			// copy data to outBuffer
			for (int i = 0; i < bufferSize; i++)
			{
				double val = man.Voices[0].Process();
				outLeft[i] = val;
				outRight[i] = val;
			}
		}


		public void OpenEditor(IntPtr parentWindow)
		{
			Controller.CreateView();
			DevInfo.EditorHeight = (Controller.View != null) ? (int)Controller.View.Height : 400;
			DevInfo.EditorWidth = (Controller.View != null) ? (int)Controller.View.Width : 800;
			HostInfo.SendEvent(this, new Event() { Data = null, EventIndex = 0, Type = EventType.WindowSize });
			DeviceUtilities.DockWpfWindow(Controller.View, parentWindow);
		}

		public void CloseEditor() { }

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				Controller.SetParameter(ModuleBinding.Filter1, ParameterBinding.Cutoff, (double)ev.Data);
			}
			else if(ev.Type == EventType.Midi)
			{
				var data = (byte[])ev.Data;
				if (data[0] == 0x80)
				{
					Controller.NoteOff(data[1]);
				}
				else if (data[0] == 0x90)
				{
					if (data[2] == 0)
						Controller.NoteOff(data[1]);
					else
						Controller.NoteOn(data[1], data[2] / 127.0);
				}
				else if (data[0] == 0xE0)
				{
					int val = (data[2] << 7) + data[1];

					//int max = 0x3FFF;
					int center = 0x2000;

					double pitch = (val - center) / (double)0x2000;
					Controller.SetPitchWheel(pitch);
				}
			}
		}

		/*public void UpdateParameterDisplay(int index, double value)
		{
			Presenter.SetParameter((Binding)index, value);
			ParameterInfo[index].Display = String.Format("{0:0}", value);
			ParameterInfo[index].Value = value;
		}*/

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
			{
				this.Samplerate = samplerate;
				Controller.SetSamplerate(samplerate);
			}
		}

		public IHostInfo HostInfo { get; set; }

	}
}

