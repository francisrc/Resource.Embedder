using FluentAssertions;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class InjectedCodeTests
	{
		#region Methods

		[Test]
		public void TestAssemblyResolve()
		{
			// the german translation dll has been manually embedded as "de.resources.dll" into the current assembly
			Translation.Language.Should().Be("English");
			// make sure our localized file is deleted
			File.Exists("de\\ResourceEmbedder.Core.Tests.resources.dll").Should().BeTrue("because .Net generates resource assemblies on each build. If this test fails here, rebuild this assembly and the test will work.");
			const string temp = "de\\ResourceEmbedder.Core.Tests.resources.dll.temp";
			if (File.Exists(temp))
			{
				File.Delete(temp);
			}
			File.Move("de\\ResourceEmbedder.Core.Tests.resources.dll", temp);

			// now that we removed the (not yet loaded) German resource, hook into the resolver and ensure it does its job
			AppDomain.CurrentDomain.AssemblyResolve += InjectedResourceLoader.AssemblyResolve;
			CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("de");
			Translation.Language.Should().Be("Deutsch");
			File.Move(temp, "de\\ResourceEmbedder.Core.Tests.resources.dll");
		}

		#endregion Methods
	}
}