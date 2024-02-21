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
		using var stream = File.Open("SelfUpdate.log", FileMode.Create, FileAccess.Write);
		using var writer = new StreamWriter(stream);
		
		var pidRegex = new Regex("^--pid=([0-9]+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		var copyFromRegex = new Regex("^--copyfrom=(.+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		var copyToRegex = new Regex("^--copyto=(.+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		var postUpdateRegex = new Regex("^--postupdate=(.+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
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
			LogError(writer, "Failed to parse args. No pid given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 10;
		}

		if (copyFrom == "")
		{
			LogError(writer, "Failed to parse args. No copyfrom given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 11;
		}

		if (copyTo == "")
		{
			LogError(writer, "Failed to parse args. No copyto given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 12;
		}		
		
		if (postUpdate == "")
		{
			LogError(writer, "Failed to parse args. No postupdate given. Expected: --pid=X --copyfrom=\"X\" --copyto=\"X\" --postupdate=\"X\"");
			return 13;
		}

		if (!singleFile)
		{
			if (!Directory.Exists(copyFrom))
			{
				LogError(writer, $"The directory for copyfrom \"{copyFrom}\" does not exist.");
				return 21;
			}

			if (!Directory.Exists(copyTo))
			{
				LogError(writer, $"The directory for copyto \"{copyTo}\" does not exist.");
				return 22;
			}
		}
		else
		{
			if (!File.Exists(copyFrom))
			{
				LogError(writer, $"The file for copyfrom \"{copyFrom}\" does not exist.");
				return 23;
			}

			if (!File.Exists(copyTo))
			{
				LogError(writer, $"The file for copyto \"{copyTo}\" does not exist.");
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
			LogError(writer, $"Failed to get process {pid}: {e}");
			return 30;
		}

		try
		{
			process.Kill();
			process.WaitForExit(TimeSpan.FromSeconds(5));
		}
		catch (Exception e)
		{
			LogError(writer, $"Failed to kill {pid}: {e}");
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
				LogError(writer, $"Failed to delete directory \"{copyTo}\": {e}");
				return 40;
			}
			
			try
			{
				Utils.CopyDir(copyFrom, copyTo);
			}
			catch (Exception e)
			{
				LogError(writer, $"Failed to copy directory \"{copyFrom}\" to \"{copyTo}\": {e}");
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
				LogError(writer, $"Failed to copy file \"{copyFrom}\" to \"{copyTo}\": {e}");
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
			LogError(writer, $"Failed to start post update process \"{postUpdate}\": {e}");
			return 50;
		}

		return 0;
	}

	public static void LogError(StreamWriter fileOutput, string message)
	{
		Console.Error.WriteLine(message);
		fileOutput.WriteLine(message);
	}
}