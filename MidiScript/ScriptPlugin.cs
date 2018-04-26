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

namespace MidiScript
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
		public IMidiPlugin MidiPlugin { get; internal set; }

		// --------------- Necessary Parameters ---------------

		private readonly ScriptViewModel ViewModel;

		public double Samplerate;
		private ScriptWindow window;
		
		public ScriptPlugin()
		{
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[0];
			PortInfo = new Port[1];
			ViewModel = new ScriptViewModel() { Plugin = this };
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Script Plugin";
#if DEBUG
			DevInfo.DeviceID = DevInfo.DeviceID + " - Dev";
#endif
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 1;
			DevInfo.EditorWidth = 1;
			DevInfo.HasEditor = true;
			DevInfo.Name = "Script Plugin";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Generator;
			DevInfo.Version = 1100;
			DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);

			//PortInfo[0].Direction = PortDirection.Input;
			//PortInfo[0].Name = "Stereo Input";
			//PortInfo[0].NumberOfChannels = 2;

			PortInfo[0].Direction = PortDirection.Output;
			PortInfo[0].Name = "Stereo Output";
			PortInfo[0].NumberOfChannels = 2;

			/*for (int i = 0; i < ParameterInfo.Length; i++)
			{
				var p = new Parameter();
				p.Display = "0.5";
				p.Index = (uint)i;
				p.Name = ParameterNames[i];
				p.Steps = 0;
				p.Value = 0.5;
				ParameterInfo[i] = p;
			}*/

			// initialize all blocks
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
			ParameterInfo[param].Display = string.Format("{0:0.00}", value);
			UpdateAll();
		}

		public void SetCurrentProgram(int program)
		{
			CurrentProgram = program;
			UpdateAll();
		}


		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			if (MidiPlugin != null)
				MidiPlugin.SampleProcess((int)Samplerate, (int)bufferSize);
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
			//window.Topmost = true;
			//var handle = new WindowInteropHelper(window).Handle;
			//SetParent(handle, parentWindow);
			window.Topmost = true;
            window.ShowInTaskbar = false;
			window.Show();
			//SharpSoundDevice.DeviceUtilities.DockWpfWindow(window, parentWindow);
		}

		public void CloseEditor()
		{
			window.Close();
		}

		public bool SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				SetParam((int)ev.EventIndex, (double)ev.Data);
				return true;
			}
			if (ev.Type == EventType.Midi)
			{
				ProcessMidi((byte[])ev.Data);
				return true;
			}
			if (ev.Type == EventType.GuiEvent)
			{
			}

			return false;
		}
		
		private RoutedEventArgs MakeKeyEventArgs(GuiEvent guiEvent)
		{
			var target = Keyboard.FocusedElement;
			var routedEvent = Keyboard.KeyDownEvent;
			var vis = target as Visual;

			var key = (Key.A);
			var mods = Keyboard.Modifiers;
            var arg = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key) { RoutedEvent = routedEvent };
			return arg;
		}

		private void ProcessMidi(byte[] data)
		{
			if (MidiPlugin == null)
				return;

			ViewModel.AddMidiInMessage("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + new MidiHelper(data).ToString());
			MidiPlugin.Process(data);
        }

		public void SendMidi(byte[] data)
		{
			ViewModel.AddMidiOutMessage("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + new MidiHelper(data).ToString());
			HostInfo.SendEvent(DeviceId, new Event { Data = data, EventIndex = 0, Type = EventType.Midi });
        }

		public void SetProgramData(Program program, int index)
		{
			try
			{
				var text = Encoding.UTF8.GetString(program.Data);
				ViewModel.Script = text;
				Task.Run(() => ViewModel.Recompile());
				UpdateAll();
			}
			catch (Exception)
			{
				MessageBox.Show("Unable to load preset");
			}
		}

		public Program GetProgramData(int index)
		{
			if (ViewModel == null || ViewModel.Script == null)
				return new Program { Name = "" };

			var output = new Program();
			output.Data = Encoding.UTF8.GetBytes(ViewModel.Script);
			output.Name = "";
			return output;
		}

		public void HostChanged()
		{
			var samplerate = HostInfo.SampleRate;
			if (samplerate != this.Samplerate)
				SetSampleRate(samplerate);
		}		
	}
}
