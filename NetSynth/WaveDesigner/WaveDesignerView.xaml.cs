using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetSynth.WaveDesigner
{
	/// <summary>
	/// Interaction logic for WaveDesignedView.xaml
	/// </summary>
	public partial class WaveDesignerView : Window
	{
		public ModuleBinding Module;

		DynamicBlockManager Manager;
		public bool ModelUpdated;
		private bool LoopRunning;

		public WaveDesignerView(DynamicBlockManager manager = null)
		{
			if(manager == null)
				manager = new DynamicBlockManager();

			InitializeComponent();
			DynamicTable.Init();
			RemoveBlockView(DummyBlock);
			Manager = manager;
			Manager.Length = Convert.ToInt32(TextBoxLength.Text);
			Manager.Partials = Convert.ToInt32(TextBoxPartials.Text);

			LoopRunning = true;
			new System.Threading.Thread(new System.Threading.ThreadStart(UpdateLoop)).Start();
			this.Closing += (object sender, System.ComponentModel.CancelEventArgs e) => 
			{ 
				LoopRunning = false; 
			};

			foreach (var block in Manager.Blocks)
				AddBlockView(block);
		}

		void UpdateLoop()
		{
			while (LoopRunning)
			{
				if (ModelUpdated == true)
				{
					try
					{
						ModelUpdated = false;
						this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { Process(); }));
					}
					catch (Exception)
					{
						// catch exception on closing, but if the loop is still running, then throw it anyway
						if (LoopRunning)
							throw;
					}
				}

				System.Threading.Thread.Sleep(100);
			}
		}

		public void RemoveBlockView(DynamicBlockView view)
		{
			StackPanel.Children.Remove(view);
			ReorderViews();
		}

		public void ReorderViews()
		{
			// get all children
			var children = new List<DynamicBlockView>();
			foreach (var elem in StackPanel.Children)
				children.Add((DynamicBlockView)elem);
			
			// order by index
			children = children.OrderBy(x => x.Model.Index).ToList();

			// clear the panel
			StackPanel.Children.Clear();

			// re-insert in correct order
			foreach (var elem in children)
				StackPanel.Children.Add(elem);

		}

		private void AddClick(object sender, RoutedEventArgs e)
		{
			var block = new DynamicBlock(Manager);
			Manager.Blocks.Add(block);
			AddBlockView(block);
		}

		private void AddBlockView(DynamicBlock block)
		{
			var view = new DynamicBlockView(this);
			view.Model = block;
			// Todo work on
			//view.ShowParameters(); asd asd asd asd as // working on this, must update view from model
			StackPanel.Children.Add(view);
		}

		private void ProcessClick(object sender, RoutedEventArgs e)
		{
			Process();
		}

		public void Process()
		{
			Manager.Length = Convert.ToInt32(TextBoxLength.Text);
			Manager.Partials = Convert.ToInt32(TextBoxPartials.Text);

			foreach (DynamicBlockView view in StackPanel.Children)
			{
				view.UpdateModel();
			}

			Manager.Process(0);

			foreach (DynamicBlockView view in StackPanel.Children)
			{
				view.UpdateErrorText();
				view.UpdateWaveDisplay();
			}
		}

		private void SetWaveClick(object sender, RoutedEventArgs e)
		{
			Binding.GlobalController.SetWave(Manager.Blocks.Last().GetTimeDomainSignal(), Module, 0);
			Binding.GlobalController.SerializedData[Module] = Manager.Serialize();
		}

		private void SetTableIndex(object sender, MouseButtonEventArgs e)
		{
			

			/*foreach (DynamicBlockView view in StackPanel.Children)
				view.ShowParameters();

			Process();*/

		}
	}
}
