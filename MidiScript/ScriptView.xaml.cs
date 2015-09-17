using AudioLib.Midi;
using LowProfile.Core.Compilation;
using Microsoft.Win32;
using SharpSoundDevice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MidiScript
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class ScriptView : UserControl
	{
		private Thread scrollThread;

		public ScriptView()
		{
			InitializeComponent();
			scrollThread = new Thread(() => Scroller()) { IsBackground = true, Priority = ThreadPriority.Lowest };
			scrollThread.Start();
        }

		~ScriptView()
		{
			scrollThread.Abort();
		}

		private void Scroller()
		{
			var midiInLastCount = 0;
			var midiOutLastCount = 0;

			while (true)
			{
				MidiInListBox.Dispatcher.Invoke(() =>
				{
					if (MidiInListBox.Items.Count != midiInLastCount)
					{
						midiInLastCount = MidiInListBox.Items.Count;
						MidiInListBox.ScrollIntoView(MidiInListBox.Items[MidiInListBox.Items.Count - 1]);
					}

					if (MidiOutListBox.Items.Count != midiOutLastCount)
					{
						midiOutLastCount = MidiOutListBox.Items.Count;
						MidiOutListBox.ScrollIntoView(MidiOutListBox.Items[MidiOutListBox.Items.Count - 1]);
					}
				});

				Thread.Sleep(100);
			}
		}
	}
}
