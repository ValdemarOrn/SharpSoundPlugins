using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace NetSynth.Tests
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			var mainView = new WaveGen.WaveGenWindow();
			mainView.Wavegen = new WaveGen.SawtoothGen();
			mainView.Wavegen.SampleCount = 2048;
			mainView.Wavegen.WaveCount = 32;
			mainView.Refresh();
			mainView.Show();
		}
	}
}
