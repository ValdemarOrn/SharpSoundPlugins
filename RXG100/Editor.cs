using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AudioLib.UI;

namespace RXG100Sim
{
	public partial class Editor : Panel
	{
		// ------------------------ Static fields and bitmap storage ------------------------

		public static Bitmap SplashBitmap;

		public static Bitmap TopBarBitmap;
		public static Bitmap TopSliderBitmap;

		public static Bitmap BackgroundBitmap;
		public static Bitmap KnobsBitmap;
		public static Bitmap SwitchBitmap;
		public static Bitmap BoostBitmap;
		public static Bitmap Light1Bitmap;
		public static Bitmap Light2Bitmap;
		public static Bitmap Light3Bitmap;

		static int TopH = 30;

		static int Slider1X = 124;
		static int Slider2X = 356;
		static int SliderY = 0;

		static int KnobX1 = 124;
		static int KnobX2 = 576;
		static int KnobY1 = 34;
		static int KnobY2 = 142;
		static int KnobH = 108;
		static int KnobW = 110;

		static int BoostX = 473;
		static int BoostY = 165;

		static int Light1X = 365;
		static int Light1Y = 57;

		static int Light2X = 365;
		static int Light2Y = 175;

		static int Light3X = 475;
		static int Light3Y = 57;

		static int SwitchX = 368;
		static int SwitchY = 110;

		static int Positions = 1;

		static Editor()
		{
			SplashBitmap = Resources.SplashBg;

			TopBarBitmap = Resources.TopBar;
			TopSliderBitmap = Resources.TopSlider;

			BackgroundBitmap = Resources.RGbase;
			KnobsBitmap = Resources.RGknobs;
			SwitchBitmap = Resources.RGswitch;
			BoostBitmap = Resources.RGboost;
			Light1Bitmap = Resources.RGlight1;
			Light2Bitmap = Resources.RGlight2;
			Light3Bitmap = Resources.RGlight3;

			Positions = KnobsBitmap.Height / BackgroundBitmap.Height;
		}

		public BitmapSliderSimple InputA, InputB;
		public Knob GainA, GainB, VolA, VolB;
		public Knob BassA, BassB, MidA, MidB, TrebleA, TrebleB, PresA, PresB;
		public Switch Channel, Boost;
		public BitmapIndicator LightA, LightB, LightC;

		// map each control to its parameter index
		public Dictionary<int, object> ControlMap = new Dictionary<int, object>();

		public RXG100 Instance;

		public void MapControls()
		{
			ControlMap[RXG100.P_CHANNEL] = Channel;
			ControlMap[RXG100.P_BOOST_B] = Boost;

			ControlMap[RXG100.P_INPUT_A] = InputA;
			ControlMap[RXG100.P_INPUT_B] = InputB;
			ControlMap[RXG100.P_GAIN_A] = GainA;
			ControlMap[RXG100.P_GAIN_B] = GainB;
			ControlMap[RXG100.P_VOL_A] = VolA;
			ControlMap[RXG100.P_VOL_B] = VolB;
			ControlMap[RXG100.P_BASS_A] = BassA;
			ControlMap[RXG100.P_BASS_B] = BassB;
			ControlMap[RXG100.P_MID_A] = MidA;
			ControlMap[RXG100.P_MID_B] = MidB;
			ControlMap[RXG100.P_TREBLE_A] = TrebleA;
			ControlMap[RXG100.P_TREBLE_B] = TrebleB;
			ControlMap[RXG100.P_PRES_A] = PresA;
			ControlMap[RXG100.P_PRES_B] = PresB;
		}
		
		public Editor()
		{
			this.ClientSize = new Size(BackgroundBitmap.Width, BackgroundBitmap.Height + TopBarBitmap.Height);

			InputA = new BitmapSliderSimple(TopSliderBitmap);
			InputA.Top = SliderY;
			InputA.Left = Slider1X;

			InputB = new BitmapSliderSimple(TopSliderBitmap);
			InputB.Top = SliderY;
			InputB.Left = Slider2X;

			// -------- first 4 knobs

			GainA = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX1, KnobY1, 0, BackgroundBitmap.Height);
			GainA.Left = KnobX1;
			GainA.Top = KnobY1 + TopH;

			VolA = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX1 + KnobW, KnobY1, 0, BackgroundBitmap.Height);
			VolA.Left = KnobX1 + KnobW;
			VolA.Top = KnobY1 + TopH;


			GainB = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX1, KnobY2, 0, BackgroundBitmap.Height);
			GainB.Left = KnobX1;
			GainB.Top = KnobY1 + KnobH + TopH;

			VolB = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX1 + KnobW, KnobY2, 0, BackgroundBitmap.Height);
			VolB.Left = KnobX1 + KnobW;
			VolB.Top = KnobY1 + KnobH + TopH;

			// ------- Channel Switch

			Channel = new BitmapSwitch(SwitchBitmap, SwitchBitmap.Width / 2, SwitchBitmap.Height, true, false);
			Channel.Left = SwitchX;
			Channel.Top = SwitchY + TopH;

			// -------- Channel Indicators

			LightA = new BitmapIndicator(Light1Bitmap, Light1Bitmap.Width / 2, Light1Bitmap.Height, false, false);
			LightA.Left = Light1X;
			LightA.Top = Light1Y + TopH;

			LightB = new BitmapIndicator(Light2Bitmap, Light2Bitmap.Width / 2, Light2Bitmap.Height, false, false);
			LightB.Left = Light2X;
			LightB.Top = Light2Y + TopH;

			LightC = new BitmapIndicator(Light3Bitmap, Light3Bitmap.Width / 2, Light3Bitmap.Height, false, false);
			LightC.Left = Light3X;
			LightC.Top = Light3Y + TopH;

			// -------- Channel A Controls

			BassA = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2, KnobY1, 0, BackgroundBitmap.Height);
			BassA.Left = KnobX2;
			BassA.Top = KnobY1 + TopH;

			MidA = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2 + KnobW, KnobY1, 0, BackgroundBitmap.Height);
			MidA.Left = KnobX2 + KnobW;
			MidA.Top = KnobY1 + TopH;

			TrebleA = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2 + 2 * KnobW, KnobY1, 0, BackgroundBitmap.Height);
			TrebleA.Left = KnobX2 + 2 * KnobW;
			TrebleA.Top = KnobY1 + TopH;

			PresA = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2 + 3 * KnobW, KnobY1, 0, BackgroundBitmap.Height);
			PresA.Left = KnobX2 + 3 * KnobW;
			PresA.Top = KnobY1 + TopH;

			// -------- Channel B Controls

			BassB = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2, KnobY2, 0, BackgroundBitmap.Height);
			BassB.Left = KnobX2;
			BassB.Top = KnobY2 + TopH;

			MidB = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2 + KnobW, KnobY2, 0, BackgroundBitmap.Height);
			MidB.Left = KnobX2 + KnobW;
			MidB.Top = KnobY2 + TopH;

			TrebleB = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2 + 2 * KnobW, KnobY2, 0, BackgroundBitmap.Height);
			TrebleB.Left = KnobX2 + 2 * KnobW;
			TrebleB.Top = KnobY2 + TopH;

			PresB = new BitmapKnob(KnobsBitmap, KnobW, KnobH, Positions, KnobX2 + 3 * KnobW, KnobY2, 0, BackgroundBitmap.Height);
			PresB.Left = KnobX2 + 3 * KnobW;
			PresB.Top = KnobY2 + TopH;

			// Switches

			Boost = new BitmapSwitch(BoostBitmap, BoostBitmap.Width / 2, BoostBitmap.Height, false, false);
			Boost.Left = BoostX;
			Boost.Top = BoostY + TopH;

			MapControls();

			foreach(var ctrl in ControlMap)
			{
				if (ctrl.Value == null)
					continue;

				if (ctrl.Value as Knob != null)
				{
					((Knob)ctrl.Value).Brush = Brushes.White;
					((Knob)ctrl.Value).ValueChanged += ParameterChanged;
				}
				else if (ctrl.Value as Switch != null)
				{
					((Switch)ctrl.Value).Brush = Brushes.White;
					((Switch)ctrl.Value).OffBrush = Brushes.White;
					((Switch)ctrl.Value).ValueChanged += ParameterChanged;
				}
				else if (ctrl.Value as BitmapSliderSimple != null)
				{
					((BitmapSliderSimple)ctrl.Value).ValueChanged += ParameterChanged;
				}

				Controls.Add((Control)ctrl.Value);
			}

			Controls.Add(LightA);
			Controls.Add(LightB);
			Controls.Add(LightC);

			var about = new AboutButton();
			about.Top = 0;
			about.Left = TopBarBitmap.Width - about.Width;
			about.Click += about_Click;
			Controls.Add(about);
		}

		void about_Click(object sender, EventArgs e)
		{
			var info = Instance.DeviceInfo;
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
			foreach (var ctrl in ControlMap)
			{
				double val = Instance.ParameterInfo[ctrl.Key].Value;

				if (ctrl.Value as Knob != null)
					((Knob)ctrl.Value).Value = val;
				else if (ctrl.Value as Switch != null)
					((Switch)ctrl.Value).Value = val;
				else if (ctrl.Value as BitmapSliderSimple != null)
					((BitmapSliderSimple)ctrl.Value).Value = val;
			}

			LightA.Value = 1 - Channel.Value;
			LightB.Value = Channel.Value;

			LightC.Value = Boost.Value;
		}

		public void ParameterChanged(object sender, double val)
		{
			if (!ControlMap.Any(x => x.Value == sender))
				return;

			var kvp = ControlMap.First(x => x.Value == sender);

			Instance.SetParam(kvp.Key, val);

			// alert host of changes
			var ev = new SharpSoundDevice.Event();
			ev.Data = val;
			ev.EventIndex = kvp.Key;
			ev.Type = SharpSoundDevice.EventType.Parameter;
			Instance.HostInfo.SendEvent(Instance, ev);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.DrawImage(TopBarBitmap, 0, 0, TopBarBitmap.Width, TopH);
			e.Graphics.DrawImage(BackgroundBitmap, 0, TopH, BackgroundBitmap.Width, BackgroundBitmap.Height);
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
