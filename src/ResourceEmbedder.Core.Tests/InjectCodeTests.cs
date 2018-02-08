using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ResourceEmbedder.Core.Cecil;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class InjectCodeTests
	{
		#region Methods

		private static string AssemblyDirectory()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var codebase = new Uri(assembly.CodeBase);
			var path = codebase.LocalPath;
			return new FileInfo(path).DirectoryName;
		}

		[Test]
		public void InjectCodeIntoConsoleExe()
		{
			var file = Path.Combine(AssemblyDirectory(), "WpfTestWithInjectedCode.exe");
			if (File.Exists(file))
			{
				File.Delete(file);
			}
			File.Copy(Path.Combine(AssemblyDirectory(), "WpfTest.exe"), file);
			if (File.Exists(Path.ChangeExtension(file, "pdb")))
				File.Delete(Path.ChangeExtension(file, "pdb"));

			using (IModifyAssemblies modifer = new CecilBasedAssemblyModifier(Substitute.For<ILogger>(), file, file))
			{
				// inject the localization assembly loader hooks
				modifer.InjectModuleInitializedCode(CecilHelpers.InjectEmbeddedResourceLoader).Should().BeTrue();
			}

			// now check that assembly has actually embedded that code by using reflection to access it
			var asm = Assembly.LoadFrom(file);
			var t = asm.Types().FirstOrDefault(t2 => t2.Name == "ResourceEmbedderILInjected");
			t.Should().NotBeNull();

			var methods = t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			methods.Should().HaveCount(5);
			// currently the class we inject uses these 5 methods
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