using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PresetSelector
{
	public class MidiProgram
	{
		public string Name { get; set; }
		public int? Msb;
		public int? Lsb;
		public int? Prg;
		public List<Tuple<int, int>> Cc;

		public string CategoryKey;
		public string CategoryName;
	}

	public class Bank
	{
		public Dictionary<string, string> CategoryNames;
		public Dictionary<string, List<MidiProgram>> Programs;

		public Bank()
		{
			CategoryNames = new Dictionary<string, string>();
			Programs = new Dictionary<string, List<MidiProgram>>();
		}
	}

	public class BankLoader
	{
		public static Bank LoadBankFromFile(string file)
		{
			return LoadBank(File.ReadAllText(file));
		}

		public static Bank LoadBank(string data)
		{
			var output = new Bank();
			var lines = data.Split('\n');
			int i = 0;
			try
			{
				foreach (var line in lines)
				{
					if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
					{
						i++;
						continue;
					}

					var parsed = ParseLine(line);
					if (parsed.Item1 == "Category")
					{
						var catKey = parsed.Item3["cat"];
						var catName = parsed.Item2;
						output.CategoryNames[catKey] = catName;
						output.Programs[catKey] = new List<MidiProgram>();
					}
					else if (parsed.Item1 == "Program")
					{
						var catKey = parsed.Item3["cat"];
						var prgName = parsed.Item2;
						var msb = ReadInt(parsed.Item3, "msb");
						var lsb = ReadInt(parsed.Item3, "lsb");
						var prg = ReadInt(parsed.Item3, "prg");
						var cc = ReadCc(parsed.Item3);
						output.Programs[catKey].Add(new MidiProgram
						{
							Cc = cc,
							Name = prgName,
							Lsb = lsb,
							Msb = msb,
							Prg = prg,
							CategoryKey = catKey,
							CategoryName = output.CategoryNames[catKey]
						});
					}
					i++;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to read file, error on line {i}:\r\n{lines[i]}");
				return new Bank();
			}

			return output;
		}

		public static Bank GetDemoBank()
		{
			var data = @"
Category Cat=Cat1 # Category 111
Category Cat=Cat2 # Category 222

Program Cat=Cat1 Msb=0 Lsb=23 Prg=1 # Program 1
Program Cat=Cat1 Msb=0 Lsb=23 Prg=2 # Program 2
Program Cat=Cat1 Msb=0 Lsb=23 Prg=3 # Program 3
Program Cat=Cat2 Msb=23 Lsb=23 Prg=1 # Program 1b
Program Cat=Cat2 Msb=23 Lsb=23 Prg=2 # Program 2b 
";
			return LoadBank(data);
		}

		private static int? ReadInt(Dictionary<string, string> data, string key)
		{
			key = key.ToLower();
			if (data.ContainsKey(key))
				return int.Parse(data[key]);
			return null;
		}

		private static List<Tuple<int,int>> ReadCc(Dictionary<string, string> data)
		{
			if (!data.ContainsKey("Cc"))
				return new List<Tuple<int, int>>();

			var cc = data["Cc"];
			var ccs = cc.Split(',');
			var output = new List<Tuple<int, int>>();
			foreach (var item in ccs)
			{
				var kvp = item.Split(':');
				var key = int.Parse(kvp[0]);
				var val = int.Parse(kvp[1]);
				output.Add(Tuple.Create(key, val));
			}

			return output;
		}

		private static Tuple<string, string, Dictionary<string, string>> ParseLine(string line)
		{
			var temp = line.Split('#');
			var description = temp[1].Trim();
			var type = temp[0].Split(' ')[0];
			var dict = new Dictionary<string, string>();
			foreach (var item in temp[0].Split(' ').Where(x => x.Contains("=")))
			{
				var kvp = item.Split('=');
				dict[kvp[0].ToLower()] = kvp[1];
			}

			return Tuple.Create(type, description, dict);
		}

		/*
		 * Format:
		 * Category Cat=SFX # My Category
		 * Program Cat=SFX Msb=0 Lsb=17 Prg=18 Cc=128:55,64:15 # Hello World
		 */
	}
}
