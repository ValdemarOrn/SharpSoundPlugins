using AudioLib.Midi;
using SharpSoundDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace PresetSelector
{
	public class ScriptPlugin : IAudioDevice
	{
		public static string[] ParameterNames = new string[0];

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public int DeviceId { get; set; }
		public DeviceInfo DeviceInfo { get { return DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		public IHostInfo HostInfo { get; set; }

		// --------------- Necessary Parameters ---------------

		private readonly ScriptViewModel ViewModel;

		public double Samplerate;
		private ScriptWindow window;
		
		public ScriptPlugin()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[0];
			PortInfo = new Port[0];
			ViewModel = new ScriptViewModel() { Plugin = this };
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Preset Selector";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 550;
			DevInfo.EditorWidth = 570;
			DevInfo.HasEditor = true;
			DevInfo.Name = "Preset Selector";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Generator;
			DevInfo.Version = 1000;
			DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);
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
		}

		public void SetCurrentProgram(int program)
		{
			CurrentProgram = program;
			UpdateAll();
		}


		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			
        }

		public void ProcessSample(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
			throw new NotImplementedException();
		}

		public void OpenEditor(IntPtr parentWindow)
		{
			HostInfo.SendEvent(DeviceId, new Event() { Data = null, EventIndex = 0, Type = EventType.WindowSize });
			window = new ScriptWindow();
			window.ViewInstance.DataContext = ViewModel;
			ViewModel.CurrentDispatcher = window.Dispatcher;
			window.Topmost = true;
            window.ShowInTaskbar = false;
			window.Show();
			DeviceUtilities.DockWpfWindow(window, parentWindow);
		}

		public void CloseEditor()
		{
			window.Close();
		}

		public bool SendEvent(Event ev)
		{
			if (ev.Type == EventType.Midi)
			{
				HostInfo.SendEvent(DeviceId, ev);
				return true;
			}

			return false;
		}

		public void SendProgram(MidiProgram program, int channel)
		{
			if (program.Msb.HasValue)
			{
				var dd = new MidiHelper(MessageType.ControlChange, channel, 0, program.Msb.Value);
				HostInfo.SendEvent(DeviceId, new Event { Data = dd.Data, EventIndex = 0, Type = EventType.Midi });
			}
			if (program.Lsb.HasValue)
			{
				var dd = new MidiHelper(MessageType.ControlChange, channel, 32, program.Lsb.Value);
				HostInfo.SendEvent(DeviceId, new Event { Data = dd.Data, EventIndex = 0, Type = EventType.Midi });
			}
			if (program.Prg.HasValue)
			{
				var dd = new MidiHelper(MessageType.ProgramChange, channel, program.Prg.Value, 0);
				HostInfo.SendEvent(DeviceId, new Event { Data = dd.Data.Take(2).ToArray(), EventIndex = 1, Type = EventType.Midi });
			}

			foreach (var cc in program.Cc)
			{
				var dd = new MidiHelper(MessageType.ControlChange, channel, cc.Item1, cc.Item2);
				HostInfo.SendEvent(DeviceId, new Event { Data = dd.Data, EventIndex = 0, Type = EventType.Midi });
			}
        }

		public void SetProgramData(Program program, int index)
		{
			try
			{
				var text = Encoding.UTF8.GetString(program.Data);
				ViewModel.LoadPluginProgram(text);
				UpdateAll();
			}
			catch (Exception)
			{
				MessageBox.Show("Unable to load preset");
			}
		}

		public Program GetProgramData(int index)
		{
			try
			{
				if (ViewModel == null || ViewModel.SelectedFileContent == null)
					return new Program { Name = "" };

				var parts = new List<string>();
				parts.Add("SelectedFile " + ViewModel.SelectedFile);
				parts.Add("SelectedChannel " + ViewModel.SelectedChannel);
				parts.Add("SelectedCategory " + ViewModel.SelectedCategory.Item1);
				parts.Add("SelectedProgram " + ViewModel.Programs.IndexOf(ViewModel.SelectedProgram));
				parts.Add("");
				parts.Add(ViewModel.SelectedFileContent);

				var output = new Program();
				output.Data = Encoding.UTF8.GetBytes(string.Join("\r\n", parts));
				output.Name = "";
				return output;
			}
			catch (Exception)
			{
				MessageBox.Show("Unable to save program");
				return new Program { Name = "" };
			}
		}

		public void HostChanged()
		{
			var samplerate = HostInfo.SampleRate;
			if (samplerate != this.Samplerate)
				SetSampleRate(samplerate);
		}		
	}
}
