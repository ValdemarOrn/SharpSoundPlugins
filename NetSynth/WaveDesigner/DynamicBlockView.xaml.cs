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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetSynth.WaveDesigner
{
	/// <summary>
	/// Interaction logic for DynamicBlockView.xaml
	/// </summary>
	public partial class DynamicBlockView : UserControl
	{
		public WaveDesignerView ParentView;
		public DynamicBlock Model;

		public DynamicBlockView()
		{
			InitializeComponent();
			
			this.Height = 179;
			CodeVisible = false;
		}

		public DynamicBlockView(WaveDesignerView parent) : this()
		{
			ParentView = parent;
		}

		public void UpdateModel()
		{
			Model.SetupCode = TextBoxSetup.Text;
			Model.ProcessCode = TextBoxProcess.Text;
			Model.IsFrequencyDomain = RadioButtonFrequency.IsChecked.GetValueOrDefault();

			foreach(var kvp in ParameterKnobs)
			{
				var uiknob = kvp.Key;
				var param = kvp.Value;

				param.ValueStart = uiknob.Value;
				param.ValueStop = uiknob.Value;
			}
		}

		public void UpdateErrorText()
		{
			TextBoxError.Text = Model.ErrorText;
		}

		public void UpdateWaveDisplay()
		{
			WaveDisplay.Data = Model.GetTimeDomainSignal();
		}

		bool CodeVisible;
		private void ShowCodeClick(object sender, RoutedEventArgs e)
		{
			if(CodeVisible)
			{
				CodeVisible = false;
				ButtonShowCode.Content = "Show Code";
				Height = 179;
			}
			else
			{
				CodeVisible = true;
				ButtonShowCode.Content = "Hide Code";
				Height = 350;
			}
		}

		private void MoveUpClick(object sender, RoutedEventArgs e)
		{
			Model.Manager.MoveUp(Model);
			ParentView.ReorderViews();
		}

		private void MoveDownClick(object sender, RoutedEventArgs e)
		{
			Model.Manager.MoveDown(Model);
			ParentView.ReorderViews();
		}

		private void DeleteClick(object sender, RoutedEventArgs e)
		{
			Model.Manager.Blocks.Remove(Model);
			ParentView.RemoveBlockView(this);
		}

		private void TextBoxProcess_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			TextBox s = (TextBox)sender;

			int TabSize = 2;
			if (e.Key == Key.Tab)
			{
				String tab = new String(' ', TabSize);
				int caretPosition = s.CaretIndex;
				s.Text = s.Text.Insert(caretPosition, tab);
				s.CaretIndex = caretPosition + TabSize;
				e.Handled = true;
			}
		}

		private void NumParametersChanged(object sender, RoutedEventArgs e)
		{
			UpdateModel();

			var parameters = Model.Parameters;
			int count = 0;
			bool parsed = int.TryParse(TextBoxParams.Text, out count);

			if (!parsed)
			{
				TextBoxParams.Text = "";
				return;
			}

			Model.SetParamCount(count);
			ShowParameters();
		}

		Dictionary<NetSynth.UI.Knob, DynamicBlockParameter> ParameterKnobs = new Dictionary<UI.Knob, DynamicBlockParameter>();

		public void ShowParameters()
		{
			ParameterView.Children.Clear();
			ParameterKnobs.Clear();

			int i = 0;
			foreach(var para in Model.Parameters)
			{
				var knob = new NetSynth.UI.Knob();
				ParameterKnobs[knob] = para;

				knob.Min = para.Min;
				knob.Max =  para.Max;
				knob.Caption = para.Name;
				knob.ShowValue = true;
				knob.Height = 50;
				knob.Width = 60;
				if (para.IsInteger)
				{
					knob.Steps = (int)(para.Max - para.Min + 1);
					knob.ValueFormatter = x => String.Format("{0:0}", x);
				}

				knob.Value = para.ValueStart;
				knob.MouseDoubleClick += knob_MouseDoubleClick;
//				knob.ValueChanged += knob_ValueChanged;

				int left = (i / 2) * 65 + 5;
				int top = (i % 2 == 0) ? 5 : 55;
				knob.Margin = new Thickness(left, top, 0, 0);
				knob.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
				knob.VerticalAlignment = System.Windows.VerticalAlignment.Top;
				ParameterView.Children.Add(knob);
				i++;
			}
		}

		void knob_ValueChanged(object sender, RoutedEventArgs e)
		{
			ParentView.ModelUpdated = true;
		}

		void knob_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			UpdateModel();
			var knob = sender as NetSynth.UI.Knob;
			var window = new ParameterEditor();
			window.Parameter = ParameterKnobs[knob];
			window.ShowDialog();
			ShowParameters();
		}
	}
}
