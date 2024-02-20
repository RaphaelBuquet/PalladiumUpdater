namespace UpdaterTests;

public class ProgramTests
{
	[Test]
	public void PidMissing()
	{
		var exitCode = PalladiumUpdater.Program.Main(Array.Empty<string>());
		Assert.AreEqual(10, exitCode);
	}
	
	[Test]
	public void CopyFromMissing()
	{
		var exitCode = PalladiumUpdater.Program.Main([
			"--pid=99999999"
		]);
		Assert.AreEqual(11, exitCode);
	}
	
	[Test]
	public void CopyToMissing()
	{
		var exitCode = PalladiumUpdater.Program.Main([
			"--pid=99999999",
			"--copyfrom=\"helloworld\"",
		]);
		Assert.AreEqual(12, exitCode);
	}
		
	[Test]
	public void PostUpdateMissing()
	{
		var exitCode = PalladiumUpdater.Program.Main([
			"--pid=99999999",
			"--copyfrom=\"helloworld\"",
			"--copyto=\"helloworld\"",
		]);
		Assert.AreEqual(13, exitCode);
	}	
	
	[Test]
	public void CopyFromDoesNotExist()
	{
		var exitCode = PalladiumUpdater.Program.Main([
			"--pid=99999999",
			"--copyfrom=\"helloworld\"",
			"--copyto=\"helloworld\"",
			"--postupdate=\"finished.exe\""
		]);
		Assert.AreEqual(21, exitCode);
	}
	
	[Test]
	public void CopyToDoesNotExist()
	{
		var temp = Path.GetTempPath();
		var exitCode = PalladiumUpdater.Program.Main([
			"--pid=99999999",
			$"--copyfrom=\"{temp}\"",
			"--copyto=\"helloworld\"",
			"--postupdate=\"finished.exe\""
		]);
		Assert.AreEqual(22, exitCode);
	}
	
}