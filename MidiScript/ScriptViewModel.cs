using AudioLib.Midi;
using LowProfile.Core.Compilation;
using LowProfile.Core.Ui;
using Microsoft.Win32;
using SharpSoundDevice;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MidiScript
{
	public class ScriptViewModel : ViewModelBase
	{
		private string selectedFile;
		private string script;
		private ObservableCollection<string> midiInMessages;
		private ObservableCollection<string> midiOutMessages;

		public ScriptViewModel()
		{
			NewScriptCommand = new DelegateCommand(_ => NewScript());
			LoadScriptCommand = new DelegateCommand(_ => SelectAndLoadScript());
			SaveScriptCommand = new DelegateCommand(_ => SaveScript());
			RecompileScriptCommand = new DelegateCommand(_ => Task.Run(() => Recompile()));
			midiInMessages = new ObservableCollection<string>();
			midiOutMessages = new ObservableCollection<string>();
        }

		public ICommand NewScriptCommand { get; private set; }
		public ICommand LoadScriptCommand { get; private set; }
		public ICommand SaveScriptCommand { get; private set; }
		public ICommand RecompileScriptCommand { get; private set; }

		public Dispatcher CurrentDispatcher { get; set; }
		public ScriptPlugin Plugin { get; set; }

		public string SelectedFile
		{
			get { return selectedFile; }
			set { selectedFile = value;  NotifyPropertyChanged(); }
		}

		public string Script
		{
			get { return script; }
			set { script = value; NotifyPropertyChanged(); }
		}

		public ObservableCollection<string> MidiInMessages
		{
			get { return midiInMessages; }
			set { midiInMessages = value;  NotifyPropertyChanged(); }
		}

		public ObservableCollection<string> MidiOutMessages
		{
			get { return midiOutMessages; }
			set { midiOutMessages = value; NotifyPropertyChanged(); }
		}

		private void NewScript()
		{
			var script = LowProfile.Core.Utils.ResourceReader.GetResourceString("MidiScript.SampleScripts.EmptyScript.cs");
			Script = script;
			SelectedFile = null;
        }

		private void SaveScript()
		{
			var dialog = new SaveFileDialog();
			dialog.Filter = "C Sharp file | *.cs";
			dialog.DefaultExt = "cs";

			if (dialog.ShowDialog() != true)
				return;

			var file = dialog.FileName;
			File.WriteAllText(file, Script);
        }

		private void SelectAndLoadScript()
		{
			var dialog = new OpenFileDialog();
			if (dialog.ShowDialog() != true)
				return;

			try
			{
				var file = dialog.FileName;
				if (!File.Exists(file))
				{
					MessageBox.Show("Selected file cannot be found.");
					return;
				}

				Script = File.ReadAllText(file);
				SelectedFile = file;
			}
			catch (Exception ex)
			{
				MessageBox.Show("An error occurred: " + ex.Message);
			}

			Task.Run(() => Recompile());
		}

		public void Recompile()
		{
			var file = SelectedFile;

			try
			{
				SelectedFile = "Compiling Script...";
				if (string.IsNullOrWhiteSpace(Script))
				{
					Plugin.MidiPlugin = null;
					return;
				}

				System.Reflection.Assembly.Load("AudioLib");
				var asm = RuntimeCompilation.CompileFile(Script);
				var type = asm.GetExportedTypes().First();
				var instance = (IMidiPlugin)Activator.CreateInstance(type);
				instance.Send = bytes => Plugin.SendMidi(bytes);
				Plugin.MidiPlugin = instance;
			}
			catch (CompilationException ce)
			{
				var errors = string.Join("\n", ce.Failures);
				MessageBox.Show(errors);
			}
			finally
			{
				SelectedFile = file;
			}
		}

		public void AddMidiInMessage(string message)
		{
			if (CurrentDispatcher == null)
				return;

			CurrentDispatcher.InvokeAsync(() => 
			{
				midiInMessages.Add(message);
				while (midiInMessages.Count > 1000)
					midiInMessages.RemoveAt(0);
			});
		}

		public void AddMidiOutMessage(string message)
		{
			if (CurrentDispatcher == null)
				return;

			CurrentDispatcher.InvokeAsync(() =>
			{
				midiOutMessages.Add(message);
				while (midiOutMessages.Count > 1000)
					midiOutMessages.RemoveAt(0);
			});
		}
	}
}
