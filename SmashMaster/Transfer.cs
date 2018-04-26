using AudioLib;
using AudioLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmashMaster
{
	public class Gain : TransferVariable
	{
		public const int P_GAIN = 0;

		public Gain (double fs) : base(fs, 1)
		{ }

		public override void Update()
		{
			double R1 = 100e3 * Parameters[P_GAIN];
			double R2 = 3.3e3;
			double c1 = 100e-12;
			double c2 = 0.047e-6;

			double[] sb = {1, (c1*R1 + c2*(R1 + R2)), c1*c2*R1*R2};
			double[] sa = {1, (c1*R1 + c2*R2), c1*c2*R1*R2};

			double[] zb, za;
			Bilinear.Transform(sb, sa, out zb, out za, Fs);

			this.B = zb;
			this.A = za;
		}
	}

	public class postGain : TransferVariable
	{
		public const int P_GAIN = 0;

		public postGain (double fs) : base (fs, 1)
		{ }

		public override void Update()
		{
			double R1 = 100e3 * (1 - Parameters[P_GAIN]);
			double R2 = 8.2e3;
			double c1 = 0.068e-6;

			double[] sb = {0, c1*R2};
			double[] sa = {1, c1*(R1 + R2)};

			double[] zb, za;
			Bilinear.Transform(sb, sa, out zb, out za, Fs);

			this.B = zb;
			this.A = za;
		}
	}

	public class TF2 : TransferVariable
	{
		public TF2 (double fs) : base(fs, 0)
		{
			
		}

		public override void Update()
		{
			double R1 = 220e3;
			double R2 = 40e3;
			double c1 = 0.0022e-6;
			double c2 = 0.22e-6;

			double[] sb = {1, (c1*R1 + c2*(R1 + R2)), c1*c2*R1*R2};
			double[] sa = {1, (c1*R1 + c2*R2), c1*c2*R1*R2};

			double[] zb, za;
			Bilinear.Transform(sb, sa, out zb, out za, Fs);

			this.B = zb;
			this.A = za;
		}
	}

	public class Contour : TransferVariable
	{
		public const int P_CONTOUR = 0;

		public Contour (double fs) : base(fs, 1)
		{  }

		public override void Update()
		{
			var contour = Parameters[P_CONTOUR];
			if (contour < 0.02)
				contour = 0.02;

			double R1 = 100;
			double R2 = 33e3;
			double R3 = 33e3;
			double c1 = 0.1e-6;
			double c2 = 0.047e-6;
			double c3 = 0.22e-6;
			double c4 = 0.001e-6;
			double RO = 100e3;
			double P1 = 100e3 * contour;
			double P2 = 100e3 * (1 - contour);

			double b4 = (c1*c2*c3*c4*P1*R2*R3*RO + c1*c2*c3*c4*P1*P2*(R2 + R3)*RO);
			double b3 = (c1*c2*c4* P1* R2 *R3 + c1* c2 *c4* P1* P2* (R2 + R3) + c1* c2* c3 *P1 *P2* RO + c2* c3 *c4 *R2 *R3* RO + c1 *c3* c4* P1* (R2 + R3)* RO + c2* c3* c4* P2* (R2 + R3)* RO);
			double b2 = (c1* c2* P1* P2 + c2* c4* R2* R3 + c1* c4* P1* (R2 + R3) + c2* c4* P2* (R2 + R3) + c1* c3* P1* RO + c2* c3* P2* RO + c3* c4* (R2 + R3)* RO);
			double b1 = (c1* P1 + c2* P2 + c4* (R2 + R3) + c3* RO);
			double b0 = 1;

			double a4 = c1* c2* c3* c4* (P1 *R1 *(R2* R3 + P2* (R2 + R3)) + (P1* (P2 + R1)* R2 + R1* R2* R3 + P1 *(P2 + R1 + R2)* R3 + P2* R1 *(R2 + R3))* RO);
			double a3 = (c2* c3* c4* (R1* R2* R3 + R2* R3* RO + R1* (R2 + R3)* RO + P2* (R2 + R3)* (R1 + RO)) + c1* (c3* c4* (R2 + R3)* (R1* RO + P1* (R1 + RO)) + c2* (c4* (P2* R1 + P1* (P2 + R1))* R2 + c4 *(R1* (P2 + R2) + P1 *(P2 + R1 + R2))* R3 + c3* (R1* (R2* (R3 + RO) + P2* (R2 + R3 + RO)) + P1* ((R1 + R2)* (R3 + RO) + P2* (R1 + R2 + R3 + RO))))));
			double a2 = (c3* c4 *(R2 + R3)* (R1 + RO) + c1* (c2* (R1* (P2 + R2) + P1* (P2 + R1 + R2)) + c4* (P1 + R1)* (R2 + R3) + c3* (R1* (R2 + R3 + RO) + P1* (R1 + R2 + R3 + RO))) + c2* (c4 *(P2 + R1)* R2 + c4* (P2 + R1 + R2)* R3 + c3* ((R1 + R2)* (R3 + RO) + P2* (R1 + R2 + R3 + RO))));
			double a1 = (c3 *R1 + c1* (P1 + R1) + c3* R2 + c4* R2 + c2 *(P2 + R1 + R2) + c3* R3 + c4* R3 + c3* RO);
			double a0 = 1;

			double[] sb = {b0,b1,b2,b3,b4};
			double[] sa = {a0,a1,a2,a3,a4};

			double[] zb, za;
			Bilinear.Transform(sb, sa, out zb, out za, Fs);

			this.B = zb;
			this.A = za;
		}
	}
}
