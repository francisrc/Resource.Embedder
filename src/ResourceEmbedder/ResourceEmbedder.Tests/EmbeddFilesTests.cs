using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using TestHelper;

namespace ResourceEmbedder.Tests
{
	[TestFixture]
	public class EmbeddFilesTests
	{
		#region Methods

		[Test]
		public void TestEmbeddTextFileInConsoleApplication()
		{
			var logger = Substitute.For<ILogger>();
			IEmbedFiles embedder = new CecilBasedEmbedder(logger);
			var dir = new FileInfo(Assembly.GetExecutingAssembly().GetLocation()).DirectoryName;
			var file = Path.Combine(dir, "ConsoleTest.exe");

			File.Exists(file).Should().BeTrue("because we referenced it.");

			var tempFile = Path.GetTempFileName();
			File.Copy(file, tempFile, true);
			var fileToEmbed = Path.Combine(RepositoryLocator.Locate(RepositoryDirectory.TestFiles), "test.txt");
			var resources = new[]
			{
				new ResourceInfo(fileToEmbed, "ConsoleTest.subfolder.test.txt"),
				new ResourceInfo(fileToEmbed, "TotallyDifferentName.test.txt")
			};
			embedder.EmbedResources(tempFile, resources).Should().BeTrue();

			var bytes = File.ReadAllBytes(tempFile);
			File.Delete(tempFile);
			// ensure the files are actually in the assembly
			var consoleApp = Assembly.Load(bytes);
			var names = consoleApp.GetManifestResourceNames();
			names.Should().Contain(new[]
			{
				"ConsoleTest.subfolder.test.txt",
				"TotallyDifferentName.test.txt"
			});

			var tempFile2 = Path.GetTempFileName();
			using (var extractedFile = consoleApp.GetManifestResourceStream("ConsoleTest.subfolder.test.txt"))
			using (var fs = new FileStream(tempFile2, FileMode.Truncate))
			{
				extractedFile.Should().NotBeNull();
				extractedFile.CopyTo(fs);
			}
			File.ReadAllText(tempFile2).Should().Be("Hello world!");
			File.Delete(tempFile2);
		}

		#endregion Methods
	}
}