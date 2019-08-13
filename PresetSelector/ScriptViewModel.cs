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

namespace PresetSelector
{
	public class ScriptViewModel : ViewModelBase
	{
		private static bool SetupComplete;

		private string selectedFile;
		private string selectedFileContent;
		private Bank bank;
		private Tuple<string, string>[] categories;
		private List<MidiProgram> programs;
		private Tuple<string, string> selectedCategory;
		private MidiProgram selectedProgram;
		private int[] channels;
		private int channel;

		public ScriptViewModel()
		{
			if (!SetupComplete)
				Logging.SetupLogging();

			LoadBankCommand = new DelegateCommand(_ => LoadBank());
			Channels = Enumerable.Range(0, 16).Select(x => x + 1).ToArray();
			SelectedChannel = 1;
		}

		public ICommand LoadBankCommand { get; private set; }

		public Dispatcher CurrentDispatcher { get; set; }
		public ScriptPlugin Plugin { get; set; }

		public string SelectedFile
		{
			get { return selectedFile; }
			set { selectedFile = value; NotifyPropertyChanged(); }
		}

		public string SelectedFileContent
		{
			get { return selectedFileContent; }
			set { selectedFileContent = value; }
		}

		public Tuple<string, string>[] Categories
		{
			get { return categories; }
			set { categories = value; NotifyPropertyChanged(); }
		}

		public List<MidiProgram> Programs
		{
			get { return programs; }
			set { programs = value; NotifyPropertyChanged(); }
		}

		public Tuple<string, string> SelectedCategory
		{
			get { return selectedCategory; }
			set { selectedCategory = value; NotifyPropertyChanged(); SetCategory(); }
		}

		public MidiProgram SelectedProgram
		{
			get { return selectedProgram; }
			set { selectedProgram = value; NotifyPropertyChanged(); SetProgram(); }
		}

		public int[] Channels
		{
			get { return channels; }
			set { channels = value; NotifyPropertyChanged(); }
		}

		public int SelectedChannel
		{
			get { return channel; }
			set { channel = value; NotifyPropertyChanged(); }
		}

		private void SetCategory()
		{
			if (selectedCategory != null)
			{
				var programs = bank.Programs[selectedCategory.Item1];
				Programs = programs.ToList();
			}
		}

		private void SetProgram()
		{
			var prg = selectedProgram;
			if (prg == null)
				return;

			Plugin.SendProgram(prg, SelectedChannel - 1);
		}

		private void LoadBank()
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

				SelectedFile = file;
				selectedFileContent = File.ReadAllText(file);
				LoadBankData();
			}
			catch (Exception ex)
			{
				MessageBox.Show("An error occurred: " + ex.Message);
			}
		}

		public void LoadBankData()
		{
			bank = BankLoader.LoadBank(selectedFileContent);
			Categories = bank.CategoryNames.Select(x => Tuple.Create(x.Key, x.Value)).ToArray();
			if (Categories.Length > 0)
				SelectedCategory = Categories[0];
			else
				SelectedCategory = null;
		}

		internal void LoadPluginProgram(string text)
		{
			var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
			var headers = lines.TakeWhile(x => x != "").ToDictionary(x => x.Split(' ')[0], x => x.Substring(x.IndexOf(' ')).Trim());
			var data = lines.Skip(headers.Count + 1);

			SelectedFile = headers["SelectedFile"];
			SelectedChannel = int.Parse(headers["SelectedChannel"]);
			SelectedFileContent = string.Join("\r\n", data);
			LoadBankData();
			SelectedCategory = Categories.First(x => x.Item1 == headers["SelectedCategory"]);
			SelectedProgram = Programs[int.Parse(headers["SelectedProgram"])];
			Task.Delay(500).ContinueWith(_ => SetProgram());
		}
	}
}
