using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.Core.Tests
{
    [TestFixture]
    public class PluginTests
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
        public void TestLoadingPluginWorksAsWell()
        {
            // this unit test project references both the console as well as the plugin the console is supposed to load, thus ensuring that both are in the output directory
            var exe = Path.Combine(AssemblyDirectory(), "PluginLoaderConsole.exe");
            File.Exists(exe).Should().BeTrue();
            var plugin = Path.Combine(AssemblyDirectory(), "LocalizedPlugin.dll");
            File.Exists(plugin).Should().BeTrue();

            var loc = Path.Combine(AssemblyDirectory(), "de\\LocalizedPlugin.resources.dll");
            if (File.Exists(loc))
            {
                File.Delete(loc);
            }
            // now we make sure the resource file of the plugin does not exist, forcing the plugin to load the embedded resource
            File.Exists(loc).Should().BeFalse();

            var p = Process.Start(exe, string.Format("\"/fulltest:{0}\"", AssemblyDirectory()));
            p.WaitForExit(2000).Should().BeTrue();
            p.ExitCode.Should().Be(0);
        }

        #endregion Methods
    }
}