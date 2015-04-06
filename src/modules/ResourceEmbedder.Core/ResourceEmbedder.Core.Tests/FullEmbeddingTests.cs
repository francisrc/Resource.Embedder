using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ResourceEmbedder.Core.Cecil;
using System.Diagnostics;
using System.IO;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class FullEmbeddingTests
	{
		#region Methods

		[Test]
		public void TestEmbedResourceAndInjectCode()
		{
			const string file = "WpfFullTest.exe";
			if (File.Exists(file))
			{
				File.Delete(file);
			}
			File.Copy("WpfTest.exe", file);

			var logger = Substitute.For<ILogger>();
			using (IModifyAssemblies modifer = new CecilBasedAssemblyModifier(logger, file, file))
			{
				var resources = new[]
				{
					new ResourceInfo("de\\WpfTest.resources.dll", "WpfTest.de.resources.dll"),
					new ResourceInfo("fr\\WpfTest.resources.dll", "WpfTest.fr.resources.dll")
				};
				modifer.EmbedResources(resources).Should().BeTrue();

				modifer.InjectModuleInitializedCode(CecilHelpers.InjectEmbeddedResourceLoader).Should().BeTrue();
			}

			// assert that the resource is embedded and that it automatically localizes using the injected code
			var info = new ProcessStartInfo(file, "/testFullyProcessed");
			using (var p = Process.Start(info))
			{
				p.Should().NotBeNull();
				p.WaitForExit(3 * 1000).Should().BeTrue();
				p.ExitCode.Should().Be(0, "because all localized files have been loaded");
			}
			File.Delete(file);
		}

		#endregion Methods
	}
}