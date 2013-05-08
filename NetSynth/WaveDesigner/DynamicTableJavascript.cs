using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSynth.WaveDesigner
{
	class DynamicTableJavascript
	{
		public static string Document =
 "<html>\n" +
 "	<head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" /></head>\n" +
 "	<script type=\"text/javascript\">\n" +
@"		
		function Process(len, waveIndex, inputSignal, inputPhase, params, setupCode, processCode)
		{
			// inputPhase can only be passed in frequency-domain execution
		
			// evaluate code blocks
			// Note, the IE8 Jscript engine has a hard time evaluating (function(){}).
			// it doesn't understand that this should return a function
			// as a workaround í use var x = function(){}; x

			try { var setupCodeblock = eval('var x = function(){' + setupCode + '}; x'); }
			catch(err) { return 'Exception: Exception while evaluating setup code. \n' + err.message; }

			try { var processCodeblock = eval('var x = function(i){ with(Math){ ' + processCode + '}}; x'); }
			catch(err) { return 'Exception: Exception while evaluating process code. \n' + err.message; }
			
			try
			{
				// Needed to translate the objects passed from C# into javascript arrays
				if(inputSignal !== null && inputSignal !== undefined)
					inputSignal = inputSignal.toArray();
				else
				{
					inputSignal = new Array(len);
					for (var q = 0; q < len; q++) inputSignal[q] = 0.0;
				}
			}
			catch(err) { return 'Exception: Exception while evaluating input signal. \n' + err.message; }
			
			try
			{
				if(inputPhase !== null && inputPhase !== undefined)
					inputPhase = inputPhase.toArray();
				else
				{
					inputPhase = new Array(len);
					for (var q = 0; q < len; q++) inputPhase[q] = 0.0;
				}
			}
			catch(err) { return 'Exception: Exception while evaluating input phase. \n' + err.message; }
				
			try
			{
				if(params !== null && params !== undefined)
					params = params.toArray();
				else
					params = [];
			}
			catch(err) { return 'Exception: Exception while evaluating input parameters. \n' + err.message; }
			
			try
			{
				// run setup code
				var data = setupCodeblock();
			}
			catch(err) { return 'Exception: Exception while running setup code. \n' + err.message; }

			var i = 0;
			
			var outputSignal = new Array(len);
			var outputPhase = inputPhase.slice(0); // creates a copy of the array

			for(i = 0; i < len; i++)
			{
				try { var val = processCodeblock(i); }
				catch(err) { return 'Exception: Exception while running process code. \n' + err.message; }
				
				if(val === undefined || val === null)
					return 'Exception: Error while processing output for i = ' + i + ': Value was undefined or null.';

				try
				{
					if(val.length === 2)
					{
						outputSignal[i] = val[0]
						outputPhase[i] = val[1];
					}
					else
					{
						outputSignal[i] = val;
					}
				}
				catch(err) { return 'Exception: Exception while processing code output. \n' + err.message; }
			}

			try
			{
				return outputSignal.toString() + ', ' + outputPhase.toString();
			}
			catch(err) { return 'Exception: Exception while trying to return results \n' + err.message; }
		}

	</script>
</html>
";

	}
}
