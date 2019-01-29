using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowProfile.Visuals;
using NUnit.Framework;

namespace AmpPlugin.Tests
{
	[TestFixture]
	class TestGainStage
	{
		[Test]
		[STAThread]
		public void Test1()
		{
			var g = new GainStage();
			g.Update(12000, 48000);
			var data = Enumerable.Range(0, 2000).Select(x => Math.Sin(x / 127.3) * 1.2).ToArray();
			var output = new double[data.Length];
			for (var i = 0; i < data.Length; i++)
			{
				output[i] = g.Compute(data[i]);
			}
			var plt = new OxyPlot.PlotModel();
			plt.AddLine(data);
			plt.AddLine(output);
			plt.Show();
		}
	}
}
