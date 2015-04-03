using FluentAssertions;
using Modules.TestHelper;
using NSubstitute;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class EmbeddFilesTests
	{
		#region Methods

		[Test]
		public void TestEmbeddMultipleLocalizationsIntoWpfExe()
		{
			var resources = new[]
			{
				new ResourceInfo("de\\WpfTest.resources.dll", "WpfTest.de.WpfTest.Resources.dll"),
				new ResourceInfo("fr\\WpfTest.resources.dll", "WpfTest.fr.WpfTest.Resources.dll")
			};
			using (var asm = EmbedHelper("WpfTest.exe", resources))
			{
				asm.Assembly.GetManifestResourceNames().Should().Contain(new[]
				{
					"WpfTest.de.WpfTest.Resources.dll",
					"WpfTest.fr.WpfTest.Resources.dll"
				});
				// ensure localization still works
				var info = new ProcessStartInfo("WpfTest.exe", "/throwOnMissingInlineLocalization");
				var p = Process.Start(info);
				p.WaitForExit(10 * 1000).Should().BeTrue();
				p.ExitCode.Should().Be(0, "because all localized files have been loaded");
			}
		}

		[Test]
		public void TestEmbeddTextFileInConsoleApplication()
		{
			var fileToEmbed = Path.Combine(RepositoryLocator.Locate(RepositoryDirectory.TestFiles), "test.txt");
			var resources = new[]
			{
				new ResourceInfo(fileToEmbed, "ConsoleTest.subfolder.test.txt"),
				new ResourceInfo(fileToEmbed, "TotallyDifferentName.test.txt")
			};
			using (var helper = EmbedHelper("ConsoleTest.exe", resources))
			{
				var names = helper.Assembly.GetManifestResourceNames();
				names.Should().Contain(new[]
				{
					"ConsoleTest.subfolder.test.txt",
					"TotallyDifferentName.test.txt"
				});
				var tempFile2 = Path.GetTempFileName();
				using (var extractedFile = helper.Assembly.GetManifestResourceStream("ConsoleTest.subfolder.test.txt"))
				using (var fs = new FileStream(tempFile2, FileMode.Truncate))
				{
					extractedFile.Should().NotBeNull();
					extractedFile.CopyTo(fs);
				}
				File.ReadAllText(tempFile2).Should().Be("Hello world!");
				File.Delete(tempFile2);
			}
		}

		/// <summary>
		/// Copies the exe to a temp location, embedds the specific resource into it and then returns the loaded assembly.
		/// </summary>
		/// <param name="exeName"></param>
		/// <param name="resources"></param>
		/// <returns></returns>
		private static AssemblyHelper EmbedHelper(string exeName, ResourceInfo[] resources)
		{
			var logger = Substitute.For<ILogger>();
			IEmbedFiles embedder = new CecilBasedEmbedder(logger);
			var dir = new FileInfo(Assembly.GetExecutingAssembly().GetLocation()).DirectoryName;
			var file = Path.Combine(dir, exeName);

			File.Exists(file).Should().BeTrue("because we referenced it.");

			var tempFile = Path.GetTempFileName();
			File.Delete(tempFile);

			embedder.EmbedResources(file, tempFile, resources).Should().BeTrue();

			var bytes = File.ReadAllBytes(tempFile);

			var asm = Assembly.Load(bytes);
			return new AssemblyHelper(asm, tempFile, () =>
			{
				File.Delete(tempFile);
			});
		}

		#endregion Methods
	}
}