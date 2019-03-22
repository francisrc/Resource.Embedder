using FluentAssertions;
using Microsoft.Build.Framework;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.MsBuild.Tests
{
    public class SymbolTests
    {
        private static string AssemblyDirectory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = codebase.LocalPath;
            return new FileInfo(path).DirectoryName;
        }

        [TestCase("None", "none")]
        [TestCase("Full", "full")]
        [TestCase("PdbOnly", "pdb-only")]
        public void MsBuildBasedEmbeddingWithSymbols(string exeName, string symbols)
        {
            // copy elsewhere and ensure localization works
            var copyDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            // helper needed by all
            File.Copy(Path.Combine(AssemblyDirectory(), "LocalizeHelper.dll"), Path.Combine(copyDir, "LocalizeHelper.dll"));
            var originalExe = Path.Combine(AssemblyDirectory(), $"{exeName}.exe");
            var output = Path.Combine(copyDir, $"{exeName}.exe");
            var outputPdb = Path.ChangeExtension(output, "pdb");

            var pdb = Path.Combine(AssemblyDirectory(), $"{exeName}.pdb");
            var targetPdb = Path.Combine(AssemblyDirectory(), "UnderTest.pdb");
            if (File.Exists(targetPdb))
                File.Delete(targetPdb);
            if (symbols != "none")
                File.Copy(pdb, targetPdb);

            var languages = new[] { "de", "pl" };
            foreach (var lang in languages)
            {
                var res = Path.Combine(AssemblyDirectory(), $"{lang}\\UnderTest.resources.dll");
                File.Copy(Path.Combine(AssemblyDirectory(), $"{lang}\\{exeName}.resources.dll"), res, true);
            }

            var fakeEngine = NSubstitute.Substitute.For<IBuildEngine>();

            var task = new SatelliteAssemblyEmbedderTask
            {
                ProjectDirectory = ".",
                AssemblyPath = originalExe,
                TargetPath = output,
                BuildEngine = fakeEngine,
                References = ".",
                DebugSymbols = true,
                DebugType = symbols
            };
            task.Execute().Should().BeTrue();
            File.Exists(output).Should().BeTrue();
            File.Exists(outputPdb).Should().Be(symbols != "none");

            var p = Process.Start(output);
            p.WaitForExit(3000).Should().BeTrue();
            p.ExitCode.Should().Be(0);
            Directory.Delete(copyDir, true);
        }

        [Test]
        public void MsBuildBasedEmbeddingWithPortableSymbolsWorksInNetCore()
        {
            // copy elsewhere and ensure localization works
            var copyDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            var dir = new DirectoryInfo(AssemblyDirectory()).Name;
            // must copy from original dir as .Net Core 2.2. can't be referenced from full framework..
            // not ideal as it doesn't ensure build is up to date..
            // also must copy multiple files for .net core
            var originalDir = $"{AssemblyDirectory()}\\..\\..\\..\\testmodules\\Symbols\\NetCorePortable\\bin\\{dir}\\netcoreapp2.2";
            var originalExe = Path.Combine(originalDir, "NetCorePortable.dll");
            var output = Path.Combine(copyDir, "NetCorePortable.dll");
            var outputPdb = Path.Combine(copyDir, "NetCorePortable.pdb");
            var toCopy = Directory.GetFiles(originalDir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var i in toCopy)
            {
                File.Copy(i, Path.Combine(copyDir, Path.GetFileName(i)));
            }

            var fakeEngine = NSubstitute.Substitute.For<IBuildEngine>();

            var task = new SatelliteAssemblyEmbedderTask
            {
                ProjectDirectory = ".",
                AssemblyPath = originalExe,
                TargetPath = output,
                BuildEngine = fakeEngine,
                References = ".",
                DebugSymbols = true,
                DebugType = "portable"
            };
            task.Execute().Should().BeTrue();
            File.Exists(output).Should().BeTrue();
            File.Exists(outputPdb).Should().BeTrue();

            var p = Process.Start("dotnet", output);
            p.WaitForExit(3000).Should().BeTrue();
            p.ExitCode.Should().Be(0);
            Directory.Delete(copyDir, true);
        }

        [Test]
        public void MsBuildBasedEmbeddingAndCleanup()
        {
            var msBuild = Path.Combine(AssemblyDirectory(), "MsBuildBasedInjected.exe");
            if (File.Exists(msBuild))
            {
                File.Delete(msBuild);
            }
            var de = Path.Combine(AssemblyDirectory(), "de\\MsBuildBasedInjected.resources.dll");
            File.Copy(Path.Combine(AssemblyDirectory(), "de\\WpfTest.resources.dll"), de, true);
            // de is german in generall, de-DE is german specific to germany -> if someone has set his localitation to German (Germany) he would get de-DE, if he sets it to e.g. German (Austria) he should get "de"
            // let's ensure that both levels of localization are correctly embedded
            var deDe = Path.Combine(AssemblyDirectory(), "de-DE\\MsBuildBasedInjected.resources.dll");
            File.Copy(Path.Combine(AssemblyDirectory(), "de-DE\\WpfTest.resources.dll"), deDe, true);
            var fr = Path.Combine(AssemblyDirectory(), "fr\\MsBuildBasedInjected.resources.dll");
            File.Copy(Path.Combine(AssemblyDirectory(), "fr\\WpfTest.resources.dll"), fr, true);
            File.Copy(Path.Combine(AssemblyDirectory(), "WpfTest.exe"), msBuild);

            // delete PDB as it doesn't match the exe anyway
            if (File.Exists(Path.ChangeExtension(msBuild, "pdb")))
                File.Delete(Path.ChangeExtension(msBuild, "pdb"));

            var fakeEngine = NSubstitute.Substitute.For<IBuildEngine>();
            var task = new SatelliteAssemblyEmbedderTask
            {
                ProjectDirectory = ".",
                AssemblyPath = msBuild,
                TargetPath = Path.GetFullPath(msBuild),
                BuildEngine = fakeEngine,
                References = "."
            };
            task.Execute().Should().BeTrue();

            File.Exists(de).Should().BeTrue();
            File.Exists(deDe).Should().BeTrue();
            File.Exists(fr).Should().BeTrue();
            var cleanupTask = new SatelliteAssemblyCleanupTask
            {
                ProjectDirectory = ".",
                AssemblyPath = msBuild,
                TargetPath = Path.GetFullPath(msBuild),
                BuildEngine = fakeEngine
            };
            cleanupTask.Execute().Should().BeTrue();
            File.Exists(de).Should().BeFalse();
            File.Exists(deDe).Should().BeFalse();
            File.Exists(fr).Should().BeFalse();

            var p = Process.Start(msBuild, "/testFullyProcessed");
            p.WaitForExit(2000).Should().BeTrue();
            p.ExitCode.Should().Be(0);
            File.Delete(de);
            File.Delete(deDe);
            File.Delete(fr);

            // assert we no longer have file lock issue
            File.Exists(msBuild).Should().BeTrue();
            File.Delete(msBuild);
            File.Exists(msBuild).Should().BeFalse();
        }
    }
}
