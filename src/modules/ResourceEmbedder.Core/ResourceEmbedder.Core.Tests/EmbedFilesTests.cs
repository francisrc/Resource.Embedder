using FluentAssertions;
using Modules.TestHelper;
using NSubstitute;
using NUnit.Framework;
using ResourceEmbedder.Core.Cecil;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class EmbedFilesTests
	{
		#region Methods

		[Test]
		public void TestEmbeddMultipleLocalizationsIntoWpfExe()
		{
			var resources = new[]
			{
				new ResourceInfo("de\\WpfTest.resources.dll", "WpfTest.de.resources.dll"),
				new ResourceInfo("fr\\WpfTest.resources.dll", "WpfTest.fr.resources.dll")
			};
			using (var asm = EmbedHelper("WpfTest.exe", resources))
			{
				asm.Assembly.GetManifestResourceNames().Should().Contain(new[]
				{
					"WpfTest.de.resources.dll",
					"WpfTest.fr.resources.dll"
				});
				// ensure localization still works
				var file = asm.AssemblyLocation;
				var info = new ProcessStartInfo(file, "/throwOnMissingInlineLocalization");
				using (var p = Process.Start(info))
				{
					p.Should().NotBeNull();
					p.WaitForExit(3 * 1000).Should().BeTrue();
					p.ExitCode.Should().Be(0, "because all localized files have been loaded");
				}
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

		[Test]
		public void TestManualAssemblyResolve()
		{
			// the german translation dll has been manually embedded as "de.resources.dll" into the current assembly
			// when embedding manually, the compiler always adds the namespace, so it should be available as a resource "ResourceEmbedder.Core.Tests.de.resources.dll" which is the name we look for
			Translation.Language.Should().Be("English");
			// make sure our localized file is deleted so we don't accidently use that for localization
			File.Exists("de\\ResourceEmbedder.Core.Tests.resources.dll").Should().BeTrue("because .Net generates resource assemblies on each build. If this test fails here, rebuild this assembly and the test will work.");
			const string temp = "de\\ResourceEmbedder.Core.Tests.resources.dll.temp";
			if (File.Exists(temp))
			{
				File.Delete(temp);
			}
			File.Move("de\\ResourceEmbedder.Core.Tests.resources.dll", temp);

			// now that we removed the (not yet loaded) German resource, hook into the resolver and ensure it does its job
			// manually hook into the required event
			AppDomain.CurrentDomain.AssemblyResolve += InjectedResourceLoader.AssemblyResolve;

			// assert that German is now loaded
			Translation.Culture = new CultureInfo("de");
			Translation.Language.Should().Be("Deutsch");
			File.Move(temp, "de\\ResourceEmbedder.Core.Tests.resources.dll");
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
			IEmbedResources embedder = new CecilBasedResourceEmbedder(logger);
			var dir = new FileInfo(Assembly.GetExecutingAssembly().GetLocation()).DirectoryName;
			var file = Path.Combine(dir, exeName);

			File.Exists(file).Should().BeTrue("because we referenced it.");

			var tempFile = Path.GetTempFileName();
			File.Delete(tempFile);
			tempFile += ".exe";
			embedder.EmbedResources(file, tempFile, resources).Should().BeTrue();

			var bytes = File.ReadAllBytes(tempFile);

			var asm = Assembly.Load(bytes);
			return new AssemblyHelper(asm, tempFile, () => File.Delete(tempFile));
		}

		#endregion Methods
	}
}