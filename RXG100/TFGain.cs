using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioLib;
using AudioLib.Modules;

namespace RXG100Sim
{
	class TFGain : TransferVariable
	{
		public const int P_GAIN = 0;
		public const int P_RUETZ = 1; // in value 0 or 1

		public TFGain(float fs) : base(fs, 2)
		{ }

		public override void Update()
		{
			double R1 = 560;
			double R2 = 47 + 10000e3 * parameters[P_RUETZ]; // ~ infinite resistance if ruetz = 1
			double C1 = 4.7e-6;
			double C2 = 2.2e-6;
			double Gain = 150e3 * parameters[P_GAIN];
			double C3 = 100e-12;

			double[] sb = { 1.0f, (C1 * Gain + C2 * Gain + C3 * Gain + C1 * R1 + C2 * R2), (C1 * C2 * Gain * R1 + C1 * C3 * Gain * R1 + C1 * C2 * Gain * R2 + C2 * C3 * Gain * R2 + C1 * C2 * R1 * R2), (C1 * C2 * C3 * Gain * R1 * R2) };
			double[] sa = { 1.0f, (C3 * Gain + C1 * R1 + C2 * R2), (C1 * C3 * Gain * R1 + C2 * C3 * Gain * R2 + C1 * C2 * R1 * R2), (C1 * C2 * C3 * Gain * R1 * R2) };

			double[] zb, za;

			Bilinear.Transform(sb, sa, out zb, out za, fs);

			this.B = zb;
			this.A = za;
		}
	}
}
