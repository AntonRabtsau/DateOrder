using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DateOrder
{
	class Program
	{
		static Regex trashFilter = new Regex(@"-[a-z0-9]{20,}-v\.", RegexOptions.IgnoreCase);
		static string GetPathForFile(string basePath, string filePath, bool purge)
		{
			var fileName = Path.GetFileName(filePath);
			if (purge && trashFilter.IsMatch(fileName))
				return Path.Combine(basePath, $"__ViberTrash__", fileName);

			var date = File.GetLastWriteTime(filePath);
			
			return Path.Combine(basePath, $"{date.Year}_{date.Month:D2}_{date.Day:D2}", fileName);
		}

		static bool CheckFlag(string[] args, string key)
		{
			return !args.Any() || args.Any(x => string.Equals(x, key, StringComparison.InvariantCultureIgnoreCase));
		}

		private static void ClearDirectory(string startLocation)
		{
			foreach (var directory in Directory.GetDirectories(startLocation))
			{
				ClearDirectory(directory);
				if (Directory.GetFiles(directory).Length == 0 &&
					Directory.GetDirectories(directory).Length == 0)
				{
					Directory.Delete(directory, false);
				}
			}
		}

		static void Main(string[] args)
		{
			var order = CheckFlag(args, "-o");
			var purge = CheckFlag(args, "-p");

			try
			{
				var workingFolder = ConfigurationSettings.AppSettings["WorkingFolder"] ?? Directory.GetCurrentDirectory();
				Console.WriteLine($"Working folder is \"{workingFolder}\"");
				var orderedSubfoldersContainerPath = Path.Combine(workingFolder, "__Ordered__");
				Directory.CreateDirectory(orderedSubfoldersContainerPath);
				Console.WriteLine($"Target folder is \"{orderedSubfoldersContainerPath}\"");


				// get all teh files recursively
				foreach (var filePath in Directory.GetFiles(workingFolder, "*.*", SearchOption.AllDirectories))
				{
					if (filePath.Contains("DateOrder.exe")) // skip this program if it is there
						continue;

					var targetPath = GetPathForFile(orderedSubfoldersContainerPath, filePath, purge);
					Console.WriteLine($"Move \"{filePath}\" to \"{targetPath}\"");
					Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
					try
					{
						File.Move(filePath, targetPath);
					}
					catch { }
				}

				// clear empty folders
				ClearDirectory(workingFolder);

				Console.WriteLine("Finished");
			}
			catch(Exception e)
			{
				Console.WriteLine("Error:"+e);
				Console.ReadLine();
			}

		}
	}
}
