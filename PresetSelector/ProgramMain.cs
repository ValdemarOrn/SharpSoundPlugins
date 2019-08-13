using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresetSelector
{
	public class ProgramMain
	{
		public static void Main(string[] args)
		{
			//UpdateGm2();

			//Categorize();
		}

		private static void Categorize()
		{
			var bankRaw = File.ReadAllText(@"C:\Users\Valdemar\Desktop\XV5050 Bank.txt");
			var bank = BankLoader.LoadBank(bankRaw);
			var allInstruments = bank.Programs.SelectMany(x => x.Value).ToArray();

			var categories = new List<string>();
			var programs = new List<string>();

			foreach (var file in Directory.GetFiles(@"C:\Users\Valdemar\Desktop\xv5050\", "*.syx"))
			{
				var cat = Path.GetFileNameWithoutExtension(file).ToUpper();
				categories.Add($"Category Cat={cat} # {cat}");
				int kk = 0;
				var syx = File.ReadAllBytes(file);
				var bytes = SplitSyx(syx);
				programs.Add("");
				foreach (var program in GetPrograms(bytes))
				{
					var match = allInstruments.SingleOrDefault(x => x.Msb == program.Item1 && x.Lsb == program.Item2 && x.Prg == program.Item3);
					programs.Add($"Program Cat={cat} Msb={match.Msb} Lsb={match.Lsb} Prg={match.Prg} # {match.CategoryKey}: {match.Name}");
					kk++;
				}
			}

			File.WriteAllLines(@"C:\Users\Valdemar\Desktop\xv5050\combined.txt", categories.Concat(programs));
		}

		private static void UpdateGm2()
		{
			var bank = BankLoader.LoadBankFromFile(@"C:\Users\Valdemar\Desktop\xv5050 GM2.txt");
			var programs = new List<string>();
			foreach (var match in bank.Programs["GM"])
			{
				var cat = match.CategoryKey;
				programs.Add($"Program Cat={cat} Msb={match.Msb} Lsb={match.Lsb} Prg={match.Prg - 1} # {match.CategoryKey}: {match.Name}");
			}

			File.WriteAllLines(@"C:\Users\Valdemar\Desktop\xv5050 GM2 Fixed.txt", programs);
		}

		private static List<byte[]> SplitSyx(byte[] syx)
		{
			var output = new List<byte[]>();
			var start = 0;
			for (int i = 0; i < syx.Length; i++)
			{
				if (syx[i] == 240)
				{
					start = i;
				}
				if (syx[i] == 247)
				{
					var end = i;
					var bytes = new byte[end - start + 1];
					Array.Copy(syx, start, bytes, 0, bytes.Length);
					output.Add(bytes);
				}
			}
			return output;
		}

		private static IEnumerable<Tuple<int, int, int>> GetPrograms(List<byte[]> syx)
		{
			foreach (var prg in syx)
			{
				var msb = prg[prg.Length - 5];
				var lsb = prg[prg.Length - 4];
				var program = prg[prg.Length - 3];
				yield return Tuple.Create((int)msb, (int)lsb, (int)program);
			}
		}
	}
}
