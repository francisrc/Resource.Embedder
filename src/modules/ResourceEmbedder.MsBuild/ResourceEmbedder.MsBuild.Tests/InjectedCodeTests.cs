using FluentAssertions;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

namespace ResourceEmbedder.MsBuild.Tests
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
			File.Exists("de\\ResourceEmbedder.MsBuild.Tests.resources.dll").Should().BeTrue();
			File.Delete("de\\ResourceEmbedder.MsBuild.Tests.resources.dll");
			File.Exists("de\\ResourceEmbedder.MsBuild.Tests.resources.dll").Should().BeFalse();

			// now that we delete the (not yet loaded) German resource, hook into the resolver and ensure it does its job
			AppDomain.CurrentDomain.AssemblyResolve += InjectedResourceLoader.AssemblyResolve;
			CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("de");
			Translation.Language.Should().Be("Deutsch");
		}

		#endregion Methods
	}
}