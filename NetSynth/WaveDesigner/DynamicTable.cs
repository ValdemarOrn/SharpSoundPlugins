using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveDesigner
{
	public class DynamicTable
	{
		/// <summary>
		/// Evaluate javascript code to generate a time- or frequency domain wavetable. 
		/// Returns two arrays, one containing samples/amplitude, the other contains phase (filled with zeros in time-domain)
		/// </summary>
		/// <param name="len">the number of samples/partials to process</param>
		/// <param name="waveIndex">the index of the wave we are processing in the wavetable (0...31)</param>
		/// <param name="input">input signal feeding into this evaluation</param>
		/// <param name="phase">the phase componenet of the input signal, only used in frequency-domain</param>
		/// <param name="parameters">optional parameters to use in the process</param>
		/// <param name="setupCode">javascript code to be processed before the main loop</param>
		/// <param name="processCode">javascript code to process inside the main loop</param>
		/// <returns></returns>
		public static double[][] Evaluate(int len, int waveIndex, double[] input, double[] phase, double[] parameters, string setupCode, string processCode)
		{
			var Doc = webBrowser.Document;

			// ATTENTION: ALL array must be object arrays!!! double[] or string[] arrays fail
			// this is becaue of variance/covariance in C#. Javascript only wants object arrays!
			
			object[] inputArr = null;
			if(input != null && input.Length > 0)
				inputArr = input.Select(x => (object)x).ToArray();
			
			object[] phaseArr = null;
			if(phase != null && phase.Length > 0)
				phaseArr = phase.Select(x => (object)x).ToArray();

			object[] paramsArr = new object[0];
			if(parameters != null && parameters.Length > 0)
				paramsArr = parameters.Select(x => (object)x).ToArray();

			var start = DateTime.Now;
			var result = (string)Doc.InvokeScript("Process", new object[] { len, waveIndex, inputArr, phaseArr, paramsArr, setupCode, processCode });
			var millis = (DateTime.Now - start).TotalMilliseconds;

			if (result.StartsWith("Exception: "))
				throw new Exception(result.Substring(11));

			double[] combinedArray = result.Split(',').Select(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
			double[] signalOutput = new double[len];
			double[] phaseOutput = new double[len];

			Array.Copy(combinedArray, 0, signalOutput, 0, len);
			Array.Copy(combinedArray, len, phaseOutput, 0, len);

			double[][] output = new double[2][];
			output[0] = signalOutput;
			output[1] = phaseOutput;

			return output;
		}

		public static System.Windows.Forms.WebBrowser webBrowser;

		public static void Init()
		{
			webBrowser = new System.Windows.Forms.WebBrowser();

			var document = DynamicTableJavascript.Document;

			webBrowser.Navigate("about:blank");
			webBrowser.Document.Write(document);
		}
	}
}
