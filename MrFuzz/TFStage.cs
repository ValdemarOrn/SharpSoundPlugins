using AudioLib;
using AudioLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrFuzz
{
	class TFStage : TransferVariable
	{

		public TFStage(float fs) : base(fs, 0)
		{ }

		public override void Update()
		{
			double[] sb = { 4.545e5, 1.002e6 };
			double[] sa = { 4.545e5, 2200 };

			double[] zb, za;

			Bilinear.Transform(sb, sa, out zb, out za, fs);

			this.B = zb;
			this.A = za;
		}
	}
}
