using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace PalladiumUpdater.Protocol;

public static class SelfUpdate
{
	public static async Task InstallUpdateFromDirectory(string downloadedUpdate, string? appExe, Assembly embeddedResourceAssembly)
	{
		if (!Directory.Exists(downloadedUpdate))
		{
			throw new ArgumentException($"Directory \"{downloadedUpdate}\" does not exist.");
		}

		// support null case only for unit tests
		string? appDirectory = null;
		if (appExe != null)
		{
			appDirectory = Path.GetDirectoryName(appExe);
			if (!Directory.Exists(appDirectory))
			{
				throw new ArgumentException($"Directory \"{appDirectory}\" does not exist.");
			}
			appExe = Path.GetFullPath(appExe);
			appDirectory = Path.GetFullPath(appDirectory);
		}
		downloadedUpdate = Path.GetFullPath(downloadedUpdate);

		string resourceName = embeddedResourceAssembly.GetManifestResourceNames().Single(x => x.EndsWith("PalladiumUpdater.exe", StringComparison.OrdinalIgnoreCase));

		// copy updater to temp
		string outOfProcessUpdaterPath = Path.Combine(Path.GetTempPath(), "PalladiumUpdater.exe");
		{
			await using (Stream? stream = embeddedResourceAssembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					throw new MissingManifestResourceException();
				}
				await using (FileStream targetStream = File.Create(outOfProcessUpdaterPath))
				{
					await stream.CopyToAsync(targetStream);
				}
			}
		}

		// support null case only for unit tests
		if (appExe != null)
		{
			// start the process... that will kill the parent process!
			var psi = new ProcessStartInfo()
			{
				FileName = outOfProcessUpdaterPath,
				Arguments = $"--pid={Process.GetCurrentProcess().Id} --copyfrom=\"{downloadedUpdate}\" --copyto=\"{appDirectory}\" --postupdate=\"{appExe}\""
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
		else
		{
			throw new Exception("Update failed because no app to replace was given.");
		}
	}
}