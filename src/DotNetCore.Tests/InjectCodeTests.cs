using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ResourceEmbedder.Core;
using ResourceEmbedder.Core.Cecil;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNetCore.Tests
{
    // DO NOT switch to NUNIT for this test
    // for some reason it will always use nunit adapter 3.10 (even if you have selected another version)
    // 3.10 ships with Mono.Cecil dll 0.10.0.0 and for some reason it is loaded as the prefered one (even if directly referencing 0.10.3)
    // thus it sublty crashes on any breaking API changes but works fine for other stuff..
    [TestClass]
    public class InjectCodeTests
    {
        private static string AssemblyDirectory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = codebase.LocalPath;
            return new FileInfo(path).DirectoryName;
        }

        [TestMethod]
        public void InjectCodeIntMultiTargetedLibrary()
        {
            var file = Path.Combine(AssemblyDirectory(), "LocalizedPluginTest.dll");
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.Copy(Path.Combine(AssemblyDirectory(), "LocalizedPlugin.dll"), file);
            if (File.Exists(Path.ChangeExtension(file, "pdb")))
                File.Delete(Path.ChangeExtension(file, "pdb"));

            using (IModifyAssemblies modifer = new CecilBasedAssemblyModifier(Substitute.For<ILogger>(), file, file))
            {
                // inject the localization assembly loader hooks
                modifer.InjectModuleInitializedCode(CecilHelpers.InjectEmbeddedResourceLoader).Should().BeTrue();
            }

            // now check that assembly has actually embedded that code by using reflection to access it
            var asm = Assembly.LoadFile(file);
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
    }
}
