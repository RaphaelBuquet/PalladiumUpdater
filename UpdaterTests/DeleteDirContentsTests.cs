using PalladiumUpdater;

namespace UpdaterTests;

public class DeleteDirContentsTests
{
	[Test]
	public void DirTest()
	{
		// arrange
		var random = new Random().Next();
		string testDir = ArrangeTestDir(random.ToString());

		// act
		Utils.DeleteDirContents(testDir);
		
		// assert
		var contents = new DirectoryInfo(testDir).EnumerateFileSystemInfos().ToList();
		CollectionAssert.AreEqual(Array.Empty<FileSystemInfo>(), contents);
	}

	internal static string ArrangeTestDir(string name)
	{
		var temp = Path.GetTempPath();
		string testDir = Path.Combine(temp, name);
		if (Directory.Exists(testDir))
		{
			Directory.Delete(testDir, true);
		}
		Directory.CreateDirectory(testDir);
		Directory.CreateDirectory(Path.Combine(testDir, "123"));
		File.WriteAllText(Path.Combine(testDir, "123", "subdir_example.txt"), "Hello");
		File.WriteAllText(Path.Combine(testDir, "root_example.txt"), "Hello");
		return testDir;
	}
}