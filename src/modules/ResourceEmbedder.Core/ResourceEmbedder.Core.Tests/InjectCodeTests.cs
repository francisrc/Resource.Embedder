using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class InjectCodeTests
	{
		#region Methods

		[Test]
		public void InjectCodeIntoConsoleExe()
		{
			const string file = "WpfTestWithInjectedCode.exe";
			if (File.Exists(file))
			{
				File.Delete(file);
			}
			File.Copy("WpfTest.exe", file);

			IInjectCode injector = new CecilBasedCodeInjector(Substitute.For<ILogger>());
			injector.Inject(file, CecilHelpers.InjectEmbeddedResourceLoader).Should().BeTrue();

			// now check that assembly has embedded code by using reflection

			var asm = Assembly.LoadFrom(file);
			var t = asm.Types().FirstOrDefault(t2 => t2.Name == "ResourceEmbedderILInjected");
			t.Should().NotBeNull();

			var methods = t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			methods.Should().HaveCount(5);
			methods.Select(m => m.Name).Should().Contain(new[]
			{
				"FindMainAssembly",
				"LoadFromResource",
				"IsLocalizedAssembly",
				"AssemblyResolve",
				"Attach"
			});
		}

		#endregion Methods
	}
}