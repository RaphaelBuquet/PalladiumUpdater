using System.Text.RegularExpressions;

namespace PalladiumUpdater.Protocol;

public static partial class HttpDownloader
{
	public static readonly Regex VersionRegex = CreateVersionRegex();

	public static async Task<string> GetArchive(HttpClient client, string url, string version,  string appName, IObserver<float> progressObserver)
	{
		Console.WriteLine($"Getting archive from {url}");
		string tempPath = Path.GetTempPath();
		string tempDownloadFile = Path.Combine(tempPath, appName, "pending-archive-download");
		if (File.Exists(tempDownloadFile))
		{
			File.Delete(tempDownloadFile);
		}
		string? directory = Path.GetDirectoryName(tempDownloadFile);
		if (directory == null)
		{
			throw new Exception($"Invalid path: \"{tempDownloadFile}\"");
		}

		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
		await using (FileStream tempFileStream = File.OpenWrite(tempDownloadFile))
		{
			HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode();
			long? totalLength = response.Content.Headers.ContentLength;
			if (totalLength == null)
			{
				progressObserver.OnNext(0.1f);
			}

			var downloadTask = response.Content.CopyToAsync(tempFileStream);

			if (totalLength != null)
			{
				while (!downloadTask.IsCompleted)
				{
					await Task.Delay(TimeSpan.FromMilliseconds(100));
					double fraction = InvertLerp(0, totalLength.Value, tempFileStream.Position);
					progressObserver.OnNext((float)fraction);
				}
			}
			await downloadTask;
		}

		string finalFile = Path.Combine(tempPath, appName, $"archive-{version}");
		if (File.Exists(finalFile))
		{
			File.Delete(finalFile);
		}

		File.Move(tempDownloadFile, finalFile);
		return finalFile;
	}

	public static async Task<string?> GetLocalVersion(string appName)
	{
		string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string appDir = Path.Combine(localAppData, appName);
		if (!Directory.Exists(appDir))
		{
			return null;
		}

		string versionFile = Path.Combine(appDir, Constants.VersionFile);
		if (!File.Exists(versionFile))
		{
			return null;
		}

		string[] contents = await File.ReadAllLinesAsync(versionFile);
		if (contents.Length != 1)
		{
			return null;
		}

		Match match = VersionRegex.Match(contents[0]);
		if (!match.Success)
		{
			return null;
		}

		return contents[0];
	}

	public static void MockLocalVersion(string appName, string version)
	{
		string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string appDir = Path.Combine(localAppData, appName);
		if (!Directory.Exists(appDir))
		{
			Directory.CreateDirectory(appDir);
		}
		string versionFile = Path.Combine(appDir, Constants.VersionFile);
		File.WriteAllText(versionFile, version);
	}

	public static async Task<string?> GetLatestVersion(HttpClient client, string url)
	{
		Console.WriteLine($"Getting latest version from {url}");
		HttpResponseMessage response = await client.GetAsync(url);
		response.EnsureSuccessStatusCode();
		string contents = await response.Content.ReadAsStringAsync();
		Match match = VersionRegex.Match(contents);
		if (!match.Success)
		{
			return null;
		}

		return contents;
	}

	[GeneratedRegex(@"^v([0-9])+(\.[0-9]+)?(\.[0-9]+)?$", RegexOptions.CultureInvariant)]
	private static partial Regex CreateVersionRegex();

	private static double InvertLerp(long a, long b, long value)
	{
		if (a == b)
		{
			return 0;
		}

		var fraction = (double)(value - a) / (b - a);
		return Math.Clamp(fraction, 0, 1);
	}
}