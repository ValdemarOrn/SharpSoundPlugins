using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmpPlugin
{
	public class GainStage
	{
		private double fb, stageIn, stageOut, trainedIn;
		private double trainingSpeed = 0.7;
		private double lpFeedbackState = 0.0;
		private double lpFeedbackAlpha = 0.05;
		private double avgIters = 0;

		public double g = 10;
		public double fbLevel = 0.5;
		public double bias = 0.4;
		
		public GainStage()
		{

		}

		public void Update(double cutoffHz, double fs)
		{
			if (cutoffHz < 10)
				cutoffHz = 10;

			// Prevent going over the Nyquist frequency
			if (cutoffHz >= fs * 0.5)
				cutoffHz = fs * 0.499;

			var x = 2 * Math.PI * cutoffHz / fs;
			var nn = (2 - Math.Cos(x));
			var alpha = nn - Math.Sqrt(nn * nn - 1);

			lpFeedbackAlpha = 1 - alpha;
		}

		public double Compute(double x)
		{
			stageIn = x;
			stageOut = -99999;
			trainedIn = 0.0;
			fb = 0.0;

			int iters = 0;
			var stageInDiff = 0.0;
			var inp = 0.0;

			while (true)
			{
				var newStageIn = (x + bias) - fb * fbLevel;

				var prevStageInDiff = stageInDiff;
				stageInDiff = newStageIn - stageIn;
				stageIn = newStageIn;

				var newInp = (1.0 - trainingSpeed) * inp + trainingSpeed * stageIn;
				var inpDiff = newInp - inp;
				inp = newInp;

				if (prevStageInDiff == 0)
				{

				}
				else if (Math.Sign(stageInDiff) != Math.Sign(prevStageInDiff))
				{
					trainingSpeed *= 0.98;
					if (trainingSpeed < 0.001)
						trainingSpeed = 0.001;
				}
				else
				{
					trainingSpeed *= 1.01;
					if (trainingSpeed > 0.95)
						trainingSpeed = 0.95;
				}

				var newStageOut = Math.Tanh(g * inp);
				var stageOutDiff = newStageOut - stageOut;
				stageOut = newStageOut;
				//fb = stageOut;
				fb = (1 - lpFeedbackAlpha) * lpFeedbackState + lpFeedbackAlpha * stageOut;
				
				iters++;

				if (Math.Abs(stageInDiff) < 0.0000001 && Math.Abs(stageOutDiff) < 0.0000001 && Math.Abs(inpDiff) < 0.0000001)
					break;
			}

			avgIters = avgIters * 0.999 + iters * (1 - 0.999);
			lpFeedbackState = fb;
			return stageOut;
		}
	}
}
