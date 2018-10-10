using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ResourceEmbedder.Core.Cecil;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.Core.Tests
{
    [TestFixture]
    public class FullEmbeddingTests
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
        public void TestEmbedResourceAndInjectCode()
        {
            var file = Path.Combine(AssemblyDirectory(), "WpfFullTest.exe");
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.Copy(Path.Combine(AssemblyDirectory(), "WpfTest.exe"), file);
            if (File.Exists(Path.ChangeExtension(file, "pdb")))
                File.Delete(Path.ChangeExtension(file, "pdb"));

            var logger = Substitute.For<ILogger>();
            using (IModifyAssemblies modifer = new CecilBasedAssemblyModifier(logger, file, file))
            {
                var resources = new[]
                {
                    new ResourceInfo(Path.Combine(AssemblyDirectory(), "de\\WpfTest.resources.dll"), "WpfTest.de.resources.dll"),
                    new ResourceInfo(Path.Combine(AssemblyDirectory(), "fr\\WpfTest.resources.dll"), "WpfTest.fr.resources.dll")
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