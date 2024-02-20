namespace PalladiumUpdater;

public static class Utils
{
	public static void DeleteDirContents(string dir)
	{
		var directoryInfo = new DirectoryInfo(dir);
		foreach (DirectoryInfo info in directoryInfo.EnumerateDirectories())
		{
			info.Delete(true);
		}
		foreach (FileInfo info in directoryInfo.EnumerateFiles())
		{
			info.Delete();
		}
	}

	public static void CopyDir(string copyFrom, string copyTo)
	{
		var copyFromInfo = new DirectoryInfo(copyFrom);
		var copyToInfo = new DirectoryInfo(copyTo);

		CopyDir(copyFromInfo, copyToInfo);
	}

	private static void CopyDir(DirectoryInfo copyFromInfo, DirectoryInfo copyToInfo)
	{
		foreach (DirectoryInfo subDir in copyFromInfo.EnumerateDirectories())
		{
			string path = subDir.FullName;
			string newPath = path.Replace(copyFromInfo.FullName, copyToInfo.FullName);
			if (newPath == path)
			{
				throw new Exception($"Failed to calculate new path for \"{path}\" using \"{copyFromInfo.FullName}\" and \"{copyToInfo.FullName}\".");
			}
			Directory.CreateDirectory(newPath);
			var newDir = new DirectoryInfo(newPath);
			CopyDir(subDir, newDir);
		}

		foreach (FileInfo info in copyFromInfo.EnumerateFiles())
		{
			string path = info.FullName;
			string newPath = path.Replace(copyFromInfo.FullName, copyToInfo.FullName);
			if (newPath == path)
			{
				throw new Exception($"Failed to calculate new path for \"{path}\" using \"{copyFromInfo.FullName}\" and \"{copyToInfo.FullName}\".");
			}
			info.CopyTo(newPath);
		}

	}
}