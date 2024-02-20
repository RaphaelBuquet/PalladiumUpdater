using System.IO.Compression;

namespace PalladiumUpdater.Protocol;

public static class ArchiveHandler
{
	/// <summary>
	///     Decompress the archive using the protocol paths.
	/// </summary>
	/// <param name="downloadedArchive"></param>
	/// <param name="appName"></param>
	/// <param name="version"></param>
	/// <returns></returns>
	public static Task<string> Decompress(string downloadedArchive, string appName, string version)
	{
		return Task.Run(async () =>
		{
			await using FileStream stream = File.OpenRead(downloadedArchive);
			string decompressedDirectory = Path.Combine(Path.GetTempPath(), appName, "decompressed");
			if (Directory.Exists(decompressedDirectory))
			{
				Directory.Delete(decompressedDirectory, true);
			}

			ZipFile.ExtractToDirectory(stream, decompressedDirectory);

			string versionFile = Path.Combine(decompressedDirectory, Constants.VersionFile);
			if (!File.Exists(decompressedDirectory))
			{
				await File.WriteAllTextAsync(versionFile, version);
			}

			return decompressedDirectory;
		});
	}
}