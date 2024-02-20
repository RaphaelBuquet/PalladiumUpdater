using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace PalladiumUpdater.Protocol;

public static class SelfUpdate
{
	public static async Task InstallUpdateSingleFile(string newAppExe, string oldAppExe, Assembly embeddedResourceAssembly)
	{
		if (!File.Exists(newAppExe))
		{
			throw new ArgumentException($"File \"{newAppExe}\" does not exist.");
		}
		if (!File.Exists(oldAppExe))
		{
			throw new ArgumentException($"File \"{oldAppExe}\" does not exist.");
		}

		string outOfProcessUpdaterExe = await GetOutOfProcessUpdaterExe(embeddedResourceAssembly);

		// start the process... that will kill the parent process!
		var psi = new ProcessStartInfo()
		{
			FileName = outOfProcessUpdaterExe,
			Arguments = $"--pid={Environment.ProcessId} --copyfrom=\"{newAppExe}\" --copyto=\"{oldAppExe}\" --singlefile --postupdate=\"{oldAppExe}\""
		};
		Process? process = Process.Start(psi);
		if (process == null)
		{
			throw new Exception($"Failed to start process {psi.FileName}.");
		}

		await process.WaitForExitAsync();

		int exitCode = process.ExitCode;

		throw new Exception($"Update failed with exit code {exitCode}.");
	}

	private static async Task<string> GetOutOfProcessUpdaterExe(Assembly embeddedResourceAssembly)
	{
		string resourceName = embeddedResourceAssembly.GetManifestResourceNames().Single(x => x.EndsWith("PalladiumUpdater.exe", StringComparison.OrdinalIgnoreCase));

		// copy updater to temp
		string outOfProcessUpdaterPath = Path.Combine(Path.GetTempPath(), "PalladiumUpdater.exe");
		await using Stream? stream = embeddedResourceAssembly.GetManifestResourceStream(resourceName);
		if (stream == null)
		{
			throw new MissingManifestResourceException();
		}
		await using FileStream targetStream = File.Create(outOfProcessUpdaterPath);
		await stream.CopyToAsync(targetStream);
		return outOfProcessUpdaterPath;
	}

	public static string? FindExeInDownloadedUpdate(string downloadedUpdate)
	{
		var files = Directory.EnumerateFiles(downloadedUpdate, "*.exe", SearchOption.TopDirectoryOnly);
		string? singleMatch = files.SingleOrDefault();
		return singleMatch;
	}
}