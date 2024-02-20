// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PalladiumUpdater;

[SuppressMessage("ReSharper", "RedundantJumpStatement")]
public static class Program
{
	public static int Main(string[] args)
	{
		var pidRegex = new Regex("^--pid=([0-9]+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		var copyFromRegex = new Regex("^--copyfrom=\"(.+)\"$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		var copyToRegex = new Regex("^--copyto=\"(.+)\"$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		var postUpdateRegex = new Regex("^--postupdate=\"(.+)\"$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		var singleFileRegex = new Regex("^--singlefile$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		
		var pid = 0;
		var copyFrom = "";
		var copyTo = "";
		var postUpdate = "";
		bool singleFile = false;
		foreach (string arg in args)
		{
			{
				Match match = pidRegex.Match(arg);
				if (match.Success)
				{
					var pidAsString = match.Groups[1].ValueSpan;
					int.TryParse(pidAsString, out pid);
					continue;
				}
			}

			{
				Match match = copyFromRegex.Match(arg);
				if (match.Success)
				{
					string val = match.Groups[1].Value;
					copyFrom = val;
					continue;
				}
			}

			{
				Match match = copyToRegex.Match(arg);
				if (match.Success)
				{
					string val = match.Groups[1].Value;
					copyTo = val;
					continue;
				}
			}	
			
			{
				Match match = postUpdateRegex.Match(arg);
				if (match.Success)
				{
					string val = match.Groups[1].Value;
					postUpdate = val;
					continue;
				}
			}

			{
				singleFile |= singleFileRegex.IsMatch(arg);
			}
		}

		if (pid == default)
		{
			Console.Error.WriteLine("Failed to parse args. No pid given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 10;
		}

		if (copyFrom == "")
		{
			Console.Error.WriteLine("Failed to parse args. No copyfrom given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 11;
		}

		if (copyTo == "")
		{
			Console.Error.WriteLine("Failed to parse args. No copyto given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 12;
		}		
		
		if (postUpdate == "")
		{
			Console.Error.WriteLine("Failed to parse args. No postupdate given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 13;
		}

		if (!singleFile)
		{
			if (!Directory.Exists(copyFrom))
			{
				Console.Error.WriteLine($"The directory for copyfrom \"{copyFrom}\" does not exist.");
				return 21;
			}

			if (!Directory.Exists(copyTo))
			{
				Console.Error.WriteLine($"The directory for copyto \"{copyTo}\" does not exist.");
				return 22;
			}
		}
		else
		{
			if (!File.Exists(copyFrom))
			{
				Console.Error.WriteLine($"The file for copyfrom \"{copyFrom}\" does not exist.");
				return 23;
			}

			if (!File.Exists(copyTo))
			{
				Console.Error.WriteLine($"The file for copyto \"{copyTo}\" does not exist.");
				return 24;
			}
		}

		Process process;
		try
		{
			process = Process.GetProcessById(pid);
		}
		catch (ArgumentException e)
		{
			Console.Error.WriteLine($"Failed to get process {pid}: {e}");
			return 30;
		}

		try
		{
			process.Kill();
		}
		catch (Exception e)
		{
			Console.Error.WriteLine($"Failed to kill {pid}: {e}");
			return 31;
		}

		if (!singleFile)
		{
			try
			{
				Utils.DeleteDirContents(copyTo);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Failed to delete directory \"{copyTo}\": {e}");
				return 40;
			}
			
			try
			{
				Utils.CopyDir(copyFrom, copyTo);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Failed to copy directory \"{copyFrom}\" to \"{copyTo}\": {e}");
				return 41;
			}
		}
		else
		{
			try
			{
				File.Copy(copyFrom, copyTo, true);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Failed to copy file \"{copyFrom}\" to \"{copyTo}\": {e}");
				return 42;
			}
		}


		try
		{
			var psi = new ProcessStartInfo()
			{
				UseShellExecute = true,
				FileName = postUpdate
			};
			Process.Start(psi);
		}
		catch (Exception e)
		{
			Console.Error.WriteLine($"Failed to start post update process \"{postUpdate}\": {e}");
			return 50;
		}

		return 0;
	}
}