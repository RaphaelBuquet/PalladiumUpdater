namespace PalladiumUpdater.Protocol;

public static class AppUpdate
{
	public static void InstallUpdateFromDirectory(string downloadedUpdate, string appName)
	{
		string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);

		if (Directory.Exists(targetDirectory))
		{
			Directory.Delete(targetDirectory, true);
		}
		
		Directory.Move(downloadedUpdate, targetDirectory);
	}
}