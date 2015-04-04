using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
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
			IEmbedFiles embedder = new CecilBasedEmbedder(logger);
			var resources = new[]
			{
				new ResourceInfo("de\\WpfTest.resources.dll", "WpfTest.de.resources.dll"),
				new ResourceInfo("fr\\WpfTest.resources.dll", "WpfTest.fr.resources.dll")
			};
			embedder.EmbedResources(file, file, resources).Should().BeTrue();

			IInjectCode injector = new CecilBasedCodeInjector(logger);
			injector.Inject(file, CecilHelpers.InjectEmbeddedResourceLoader).Should().BeTrue();

			throw new NotImplementedException("TODO: finish");
			File.Delete(file);
		}

		#endregion Methods
	}
}