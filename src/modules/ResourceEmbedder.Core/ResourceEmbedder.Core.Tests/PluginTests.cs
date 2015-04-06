using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class PluginTests
	{
		[Test]
		public void TestLoadingPluginWorksAsWell()
		{
			// this unit test project references both the console as well as the plugin the console is supposed to load, thus ensuring that both are in the output directory
			const string exe = "PluginLoaderConsole.exe";
			File.Exists(exe).Should().BeTrue();
			const string plugin = "LocalizedPlugin.dll";
			File.Exists(plugin).Should().BeTrue();

			const string loc = "de\\LocalizedPlugin.resources.dll";
			if (File.Exists(loc))
			{
				File.Delete(loc);
			}
			// now we make sure the resource file of the plugin does not exist, forcing the plugin to load the embedded resource
			File.Exists(loc).Should().BeFalse();

			var p = Process.Start(exe, "/fulltest");
			p.WaitForExit(2000);
			p.ExitCode.Should().Be(0);
		}
	}
}