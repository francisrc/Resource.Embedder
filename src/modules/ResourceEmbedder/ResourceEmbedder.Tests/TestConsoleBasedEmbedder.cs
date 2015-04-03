using FluentAssertions;
using Modules.TestHelper;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace ResourceEmbedder.Tests
{
	[TestFixture]
	public class TestConsoleBasedEmbedder
	{
		#region Methods

		[Test]
		public void TestEmbedTextFileInExe()
		{
			var file = Path.Combine(RepositoryLocator.Locate(RepositoryDirectory.TestFiles), "test.txt");
			var command = string.Format("\"/input:{0}\" \"/output:{1}\" {2}>ConsoleTestWithResource.exe.test.txt", "ConsoleTest.exe", "ConsoleTestWithResource.exe", file);

			// embed file into exe
			var r = Process.Start("ResourceEmbedder.exe", command);
			r.WaitForExit(5000).Should().BeTrue();
			r.ExitCode.Should().Be(0);

			// new file should now exist
			File.Exists("ConsoleTestWithResource.exe").Should().BeTrue();

			var path = Path.GetTempFileName();
			File.Delete(path);

			// run new file with command line that will extract the resource
			var r2 = Process.Start("ConsoleTestWithResource.exe", string.Format("ConsoleTestWithResource.exe.test.txt {0}", path));
			r2.WaitForExit(5000).Should().BeTrue();
			r2.ExitCode.Should().Be(0);

			// since we embedded a text file the file that was extracted should be that same file
			File.ReadAllText(path).Should().Be("Hello world!");
		}

		#endregion Methods
	}
}