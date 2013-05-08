using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AudioLib.UI;

namespace Rodent.V2
{
	public partial class Editor : Panel
	{
		public static Bitmap TopBarBitmap;
		public static Bitmap BaseBitmap;
		public static Bitmap KnobsBitmap;
		public static Bitmap SwitchBitmap;
		public static Bitmap SplashBitmap;

		static int TopH = 30;

		// Knobs
		static int Knob1X = 75;
		static int Knob2X = 235;
		static int Knob3X = 345;

		static int KnobHeight = 125;

		static int Knob1Width = 160;
		static int Knob2Width = 110;
		static int Knob3Width = 130;

		// Switches
		static int Switch1X = 100;
		static int Switch2X = 165;
		static int LightX = 235;
		static int Switch3X = 315;
		static int Switch4X = 385;

		static int SwitchY = 125;
		static int SwitchHeight = 75;

		static int Switch1Width = 65;
		static int Switch2Width = 70;
		static int LightWidth = 80;
		static int Switch3Width = 70;
		static int Switch4Width = 65;

		// Stomp
		static int StompX = 225;
		static int StompY = 200;
		static int StompHeight = 155;
		static int StompWidth = 100;


		static int Positions = 101;

		static Editor()
		{
			SplashBitmap = Resources.SplashBg;
			TopBarBitmap = Resources.Rodent_Top;
			BaseBitmap = Resources.Rodent_Base;
			KnobsBitmap = Resources.Rodent_Knobs;
			SwitchBitmap = new Bitmap(BaseBitmap.Width, BaseBitmap.Height * 2);
			var g = Graphics.FromImage(SwitchBitmap);
			g.DrawImage(BaseBitmap, 0, 0, BaseBitmap.Width, BaseBitmap.Height);
			g.DrawImage(Resources.Rodent_Switch_On, 0, BaseBitmap.Height, BaseBitmap.Width, BaseBitmap.Height);
		}

		public Knob Gain, Filter, Vol;
		public Switch Ruetz, Turbo, Tight, OD;
		public BitmapIndicator Light;
		public Switch Stomp;

		Rodent Rodent;

		public Editor(Rodent instance)
		{
			this.Rodent = instance;

			this.ClientSize = new Size(BaseBitmap.Width, BaseBitmap.Height + TopBarBitmap.Height);

			Gain = new BitmapKnob(KnobsBitmap, Knob1Width, KnobHeight, Positions, 0, 0, 0, KnobHeight);
			Gain.Brush = Brushes.White;
			Gain.Top = TopH;
			Gain.Left = Knob1X;

			Filter = new BitmapKnob(KnobsBitmap, Knob2Width, KnobHeight, Positions, Knob1Width, 0, 0, KnobHeight);
			Filter.Brush = Brushes.White;
			Filter.Top = TopH;
			Filter.Left = Knob2X;

			Vol = new BitmapKnob(KnobsBitmap, Knob3Width, KnobHeight, Positions, Knob1Width + Knob2Width, 0, 0, KnobHeight);
			Vol.Brush = Brushes.White;
			Vol.Top = TopH;
			Vol.Left = Knob3X;



			Ruetz = new BitmapSwitch(SwitchBitmap, Switch1Width, SwitchHeight, false, true, Switch1X, SwitchY);
			Ruetz.Brush = Brushes.White;
			Ruetz.OffBrush = Brushes.White;
			Ruetz.Top = TopH + SwitchY;
			Ruetz.Left = Switch1X;

			Turbo = new BitmapSwitch(SwitchBitmap, Switch2Width, SwitchHeight, false, true, Switch2X, SwitchY);
			Turbo.Brush = Brushes.White;
			Turbo.OffBrush = Brushes.White;
			Turbo.Top = TopH + SwitchY;
			Turbo.Left = Switch2X;

			Tight = new BitmapSwitch(SwitchBitmap, Switch3Width, SwitchHeight, false, true, Switch3X, SwitchY);
			Tight.Brush = Brushes.White;
			Tight.OffBrush = Brushes.White;
			Tight.Top = TopH + SwitchY;
			Tight.Left = Switch3X;

			OD = new BitmapSwitch(SwitchBitmap, Switch4Width, SwitchHeight, false, true, Switch4X, SwitchY);
			OD.Brush = Brushes.White;
			OD.OffBrush = Brushes.White;
			OD.Top = TopH + SwitchY;
			OD.Left = Switch4X;

			Light = new BitmapIndicator(SwitchBitmap, LightWidth, SwitchHeight, false, true, LightX, SwitchY);
			Light.Left = LightX;
			Light.Top = TopH + SwitchY;

			Stomp = new BitmapSwitch(SwitchBitmap, StompWidth, StompHeight, false, true, StompX, StompY);
			Stomp.Left = StompX;
			Stomp.Top = TopH + StompY;
			Stomp.Mode = Switch.SwitchMode.Toggle;

			Controls.Add(Gain);
			Controls.Add(Filter);
			Controls.Add(Vol);
			Controls.Add(Ruetz);
			Controls.Add(Turbo);
			Controls.Add(Tight);
			Controls.Add(OD);
			Controls.Add(Light);
			Controls.Add(Stomp);

			Gain.ValueChanged += ParameterChanged;
			Filter.ValueChanged += ParameterChanged;
			Vol.ValueChanged += ParameterChanged;
			Ruetz.ValueChanged += ParameterChanged;
			Turbo.ValueChanged += ParameterChanged;
			Tight.ValueChanged += ParameterChanged;
			OD.ValueChanged += ParameterChanged;
			Stomp.ValueChanged += ParameterChanged;

			var about = new AboutButton();
			about.Top = 0;
			about.Left = TopBarBitmap.Width - about.Width;
			about.Click += about_Click;
			Controls.Add(about);
		}

		void about_Click(object sender, EventArgs e)
		{
			var info = Rodent.DeviceInfo;
			var data = new SplashData()
			{
				Developer = info.Developer,
				Plugin = info.Name,
				Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
				Website = "www.analogwindow.com"
			};

			var splash = new Splash(SplashBitmap, data);
			splash.Left = Width / 2 - splash.Width / 2;
			splash.Top = TopH + (Height - TopH) / 2 - splash.Height / 2;
			Controls.Add(splash);
			splash.BringToFront();
		}

		public void UpdateParameters()
		{
			Gain.Value = Rodent.ParameterInfo[Rodent.P_GAIN].Value;
			Filter.Value = Rodent.ParameterInfo[Rodent.P_FILTER].Value;
			Vol.Value = Rodent.ParameterInfo[Rodent.P_VOL].Value;
			Ruetz.Value = Rodent.ParameterInfo[Rodent.P_RUETZ].Value;
			Turbo.Value = Rodent.ParameterInfo[Rodent.P_TURBO].Value;
			Tight.Value = Rodent.ParameterInfo[Rodent.P_TIGHT].Value;
			OD.Value = Rodent.ParameterInfo[Rodent.P_OD].Value;
			Light.Value = Rodent.ParameterInfo[Rodent.P_ON].Value;
			Stomp.Value = Rodent.ParameterInfo[Rodent.P_ON].Value;
		}

		void SendUpdate(int paramIndex)
		{
			var ev = new SharpSoundDevice.Event();
			ev.Data = Rodent.ParameterInfo[paramIndex].Value;
			ev.EventIndex = paramIndex;
			ev.Type = SharpSoundDevice.EventType.Parameter;
			Rodent.HostInfo.SendEvent(Rodent, ev);
		}

		public void ParameterChanged(object sender, double val)
		{
			if(sender == Gain)
			{
				Rodent.SetParam(Rodent.P_GAIN, val);
				SendUpdate(Rodent.P_GAIN);
			}
			else if (sender == Filter)
			{
				Rodent.SetParam(Rodent.P_FILTER, val);
				SendUpdate(Rodent.P_FILTER);
			}
			else if (sender == Vol)
			{
				Rodent.SetParam(Rodent.P_VOL, val);
				SendUpdate(Rodent.P_VOL);
			}
			else if (sender == Ruetz)
			{
				Rodent.SetParam(Rodent.P_RUETZ, val);
				SendUpdate(Rodent.P_RUETZ);
			}
			else if (sender == Tight)
			{
				Rodent.SetParam(Rodent.P_TIGHT, val);
				SendUpdate(Rodent.P_TIGHT);
			}
			else if (sender == Turbo)
			{
				Rodent.SetParam(Rodent.P_TURBO, val);
				SendUpdate(Rodent.P_TURBO);
			}
			else if (sender == OD)
			{
				Rodent.SetParam(Rodent.P_OD, val);
				SendUpdate(Rodent.P_OD);
			}
			else if (sender == Stomp)
			{
				Rodent.SetParam(Rodent.P_ON, val);
				SendUpdate(Rodent.P_ON);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.DrawImage(TopBarBitmap, 0, 0, TopBarBitmap.Width, TopH);
			e.Graphics.DrawImage(BaseBitmap, 0, TopH, BaseBitmap.Width, BaseBitmap.Height);
			base.OnPaint(e);
		}

		private class AboutButton : Button
		{
			public AboutButton()
			{
				Width = 260;
				Height = TopBarBitmap.Height;
				Cursor = Cursors.Hand;
			}

			protected override void OnPaint(PaintEventArgs paint)
			{
				paint.Graphics.DrawImage(TopBarBitmap, new Rectangle(0, 0, Width, Height), TopBarBitmap.Width - 260, 0, 260, Height, GraphicsUnit.Pixel);
			}
		}
	}
}
