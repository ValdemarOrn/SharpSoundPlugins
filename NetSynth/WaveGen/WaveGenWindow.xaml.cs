using NetSynth.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetSynth.WaveGen
{
	/// <summary>
	/// Interaction logic for WaveGenWindow.xaml
	/// </summary>
	public partial class WaveGenWindow : Window
	{
		public Wavegen Wavegen { get; set; }
		public ModuleBinding Module;

		public bool FinishIsSelected;

		public WaveGenWindow()
		{
			InitializeComponent();
			ParameterMap = new Dictionary<WaveParameter, Knob>();

			if(Wavegen != null)
				Refresh();

			StartFinishClick(LabelStart, null);

			Processing = true;
			new Thread(new ThreadStart(ProcessLoop)).Start();
		}

		private void StartFinishClick(object sender, MouseButtonEventArgs e)
		{
			if (sender == LabelStart)
			{
				FinishIsSelected = false;
				LabelStart.FontWeight = FontWeights.Bold;
				LabelFinish.FontWeight = FontWeights.SemiBold;
			}
			else
			{
				FinishIsSelected = true;
				LabelStart.FontWeight = FontWeights.SemiBold;
				LabelFinish.FontWeight = FontWeights.Bold;
			}

			if (Wavegen == null)
				return;

			foreach(var param in Wavegen.Parameters)
			{
				if(FinishIsSelected)
					ParameterMap[param].Value = param.ValueFinish;
				else
					ParameterMap[param].Value = param.ValueStart;
			}
		}

		Dictionary<WaveParameter, Knob> ParameterMap;

		public void Refresh()
		{
			StackPanel.Children.Clear();
			ParameterMap = new Dictionary<WaveParameter, Knob>();

			foreach(var param in Wavegen.Parameters)
			{
				var box = new Border() { Margin = new Thickness(5) };
				box.BorderThickness = new Thickness(1.0);
				box.BorderBrush = Brushes.Black;

				var knob = new UI.Knob() { Height = 80, Margin = new Thickness(0, 5, 0, 0), Width = 100 };
				var label = new Label() 
				{ 
					FontSize = 16, 
					VerticalAlignment = System.Windows.VerticalAlignment.Center, 
					Content = new TextBlock() { Text = param.Name + "\n" + param.GetString(0,32) } 
				};

				ParameterMap[param] = knob;

				knob.ValueChanged += (sender, e) => 
				{
					if (!FinishIsSelected) // start
					{
						param.ValueStart = knob.Value;
						label.Content = new TextBlock() { Text = param.Name + "\n" + param.GetString(0, 32) };
					}
					else // finish
					{
						param.ValueFinish = knob.Value;
						label.Content = new TextBlock() { Text = param.Name + "\n" + param.GetString(31, 32) };
					}

					// trigger a refresh
					HasChanged = true;
				};

				var panel = new StackPanel() { Orientation = Orientation.Horizontal };
				panel.Children.Add(knob);
				panel.Children.Add(label);

				box.Child = panel;

				StackPanel.Children.Add(box);
			}
		}

		private void LoadClick(object sender, MouseButtonEventArgs e)
		{
			HasChanged = true;
		}

		public bool Processing;
		public bool HasChanged;
		public void ProcessLoop()
		{
			while (Processing)
			{
				if (HasChanged)
				{
					HasChanged = false;
					Wavegen.Process();

					for (int i = 0; i < Wavegen.WaveCount; i++)
					{
						Binding.GlobalController.SetWave(Wavegen.Waves[i], Module, i);
					}

					this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { IndexChanged(null, null); }));
				}

				Thread.Sleep(100);
			}
		}

		private void IndexChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				var index = (int)KnobIndex.Value;
				double pos = KnobIndex.Value % 1.0;

				if (index == 31)
				{
					Display.Data = Wavegen.Waves[index];
					return;
				}

				var data = new double[Wavegen.SampleCount];
				for (int i = 0; i < data.Length; i++)
				{
					data[i] = Wavegen.Waves[index][i] * (1 - pos) + Wavegen.Waves[index + 1][i] * pos;
				}

				Display.Data = data;
			}
			catch(Exception)
			{
				
			}
		}

		private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Processing = false;
		}

		
	}
}
