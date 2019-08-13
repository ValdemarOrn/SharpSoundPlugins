using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PresetSelector
{
	public class Logging
	{
		public static void SetupLogging()
		{
			if (Application.Current != null)
			{
				Application.Current.DispatcherUnhandledException += (s, e) =>
				{
					Exception("An unhandled exception has occurred.\r\n" + e.Exception.Message);
					e.Handled = true;
				};
			}

			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				var ex = e.ExceptionObject as Exception;
				Exception("An unhandled exception killed the process.\r\n" + ex.Message);
			};
		}

		public static void Exception(string message)
		{
			MessageBox.Show(message);
		}
	}
}
