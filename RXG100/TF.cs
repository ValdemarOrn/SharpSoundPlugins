using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioLib;
using AudioLib.Modules;

namespace RXG100Sim
{
	/// <summary>
	/// TF to emulate input stage (before the first JFET)
	/// </summary>
	public class TF1 : TransferVariable
	{
		public TF1(double fs) : base(fs, 0)
		{ }

		public override void Update()
		{
			double c1 = 0.01e-6f;
			double r1 = 68e3f;
			double c2 = 250e-12f;
			double r2 = 1e6f;
			
			double[] sb = {0, c1*r2, 0};
			double[] sa = {1, r2*c2+c1*r1+c1*r2, r1*r2*c1*c2};

			double[] zb, za;

			Bilinear.Transform(sb, sa, out zb, out za, this.fs);

			this.B = zb;
			this.A = za;
		}
	}

	/// <summary>
	/// TF to emulate effects of limited size bypass cap (0.47uF)
	/// </summary>
	public class TF12 : TransferVariable
	{
		public TF12(double fs) : base(fs, 0)
		{ }

		public override void Update()
		{
			double[] sb = {361.1423f, 0.4076f}; // 860hz ca
			double[] sa = {5403.5f, 1f};

			double[] zb, za;

			Bilinear.Transform(sb, sa, out zb, out za, this.fs);

			this.B = zb;
			this.A = za;
		}
	}

	// Between Jfet1 and Jfet2. Contains Gain control
	public class TF2 : TransferVariable
	{
		public const int P_GAIN = 0;
		public const int P_USE_R3 = 1;

		public TF2(double fs) : base(fs, 2)
		{ }

		public override void Update()
		{
			double c4 = 0.02e-6f;
			double c5 = 0.005e-6f;
			double r3 = 22e3f * P_USE_R3 + 100;
			double Rvol = 50e3f;
			double x = this.parameters[P_GAIN];

			double[] sb = {0, -c4*Rvol*x, c4*Rvol*x*(-c5*(Rvol + r3) + c5*Rvol*x)};
			double[] sa = {-1, (-c4*Rvol - c5*(Rvol + r3) + c5*Rvol*x), -c4*c5*Rvol*(r3 - Rvol*(-1 + x)*x)};

			double[] zb, za;

			Bilinear.Transform(sb, sa, out zb, out za, this.fs);

			this.B = zb;
			this.A = za;
		}
	}

	public class TFPres : TransferVariable
	{
		public const int P_PRES = 0;

		public TFPres(double fs) : base(fs, 1)
		{ }

		public override void Update()
		{
			double x = this.parameters[P_PRES];
			double C1 = 0.1e-6f;
			double R14 = 1e3f;
			double P = 2e3f*x;

			double[] b = {1, C1*P};
			double[] a = {1, C1*(P+R14)};

			double[] zb, za;

			Bilinear.Transform(b, a, out zb, out za, this.fs);

			this.B = zb;
			this.A = za;
		}
	}

	public class TFVolume : TransferVariable
	{
		public const int P_VOL = 0;

		public TFVolume(double fs) : base(fs, 1)
		{ }

		public override void Update()
		{
			double x = this.parameters[P_VOL];
			double C1 = 200e-9f;
			double C2 = 0.0002e-6f;
			double R2 = 100e3f*(1-x);
			double R1 = 100e3f*x;
			//double R3 = 140e3f;

			double[] b = {0, C1*R1, C1*C2*R1*R2};
			double[] a = {1, (C1*R1+C1*R2+C2*R2), C1*C2*R1*R2 };

			double[] zb, za;

			Bilinear.Transform(b, a, out zb, out za, this.fs);

			this.B = zb;
			this.A = za;
		}
	}
}
