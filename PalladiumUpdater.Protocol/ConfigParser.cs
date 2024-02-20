namespace PalladiumUpdater.Protocol;

public static class ConfigParser
{
	public static async Task<UpdateSourceConfig> Parse(string filePath)
	{
		string[] lines = await File.ReadAllLinesAsync(filePath);
		if (lines.Length != 2)
		{
			return new UpdateSourceConfig() { ArchiveUrl = "", LatestVersionUrl = "" };
		}
		return new UpdateSourceConfig()
		{
			LatestVersionUrl = lines[0],
			ArchiveUrl = lines[1]
		};
	}

	public static string MakeString(UpdateSourceConfig config)
	{
		return $"{config.LatestVersionUrl}\n{config.ArchiveUrl}";
	}
}