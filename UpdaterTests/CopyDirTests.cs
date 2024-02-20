using PalladiumUpdater;

namespace UpdaterTests;

public class CopyDirTests
{
	[Test]
	public void Test()
	{
		// arrange
		var random = new Random();
		var dir1Name = random.Next().ToString();
		var dir2Name = random.Next().ToString();
		var dir1 = Path.Combine(Path.GetTempPath(), dir1Name);
		var dir2 = Path.Combine(Path.GetTempPath(), dir2Name);
		
		DeleteDirContentsTests.ArrangeTestDir(dir1Name);
		if (Directory.Exists(dir2))
		{
			Directory.Delete(dir2, true);
		}
		Directory.CreateDirectory(dir2Name);
		
		// act
		Utils.CopyDir(dir1, dir2);
		
		// assert
		var expected = Directory.EnumerateFileSystemEntries(dir1, "*", SearchOption.AllDirectories)
			.Select(x => RemoveRoot(x, dir1))
			.ToList();
		var actual = Directory.EnumerateFileSystemEntries(dir2, "*", SearchOption.AllDirectories)
			.Select(x => RemoveRoot(x, dir2))
			.ToList();
		
		CollectionAssert.AreEqual(expected, actual);
	}

	internal static string RemoveRoot(string path, string root)
	{
		var newPath = path.Replace(root, "");
		if (newPath == path)
		{
			throw new ArgumentException();
		}
		return newPath;
	}
}