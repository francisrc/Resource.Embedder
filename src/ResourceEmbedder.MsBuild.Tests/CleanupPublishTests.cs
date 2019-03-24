using FluentAssertions;
using Microsoft.Build.Framework;
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.MsBuild.Tests
{
    [TestFixture]
    public class CleanupPublishTests
    {
        private static string AssemblyDirectory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = codebase.LocalPath;
            return new FileInfo(path).DirectoryName;
        }

        [Test]
        public void MsBuildBasedEmbeddingAndPublishCleanup()
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
            task.EmbeddedCultures.Should().ContainAll("de;", "de-DE", "fr");

            File.Exists(de).Should().BeTrue();
            File.Exists(deDe).Should().BeTrue();
            File.Exists(fr).Should().BeTrue();
            var cleanupTask = new SatelliteAssemblyCleanupPublishTask
            {
                ProjectDirectory = ".",
                AssemblyPath = msBuild,
                TargetPath = Path.GetFullPath(msBuild),
                BuildEngine = fakeEngine,
                PublishDir = AssemblyDirectory(),
                EmbeddedCultures = task.EmbeddedCultures,
                ExistingPublishPath = new[] { de, deDe, fr }.Select(i => new FakeItem(i)).ToArray()
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

        public class FakeItem : ITaskItem
        {
            private readonly string _resourceFilePath;

            public FakeItem(string resourceFilePath)
            {
                _resourceFilePath = resourceFilePath;
            }

            public string ItemSpec { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ICollection MetadataNames => throw new NotImplementedException();

            public int MetadataCount => throw new NotImplementedException();

            public IDictionary CloneCustomMetadata()
            {
                throw new NotImplementedException();
            }

            public void CopyMetadataTo(ITaskItem destinationItem)
            {
                throw new NotImplementedException();
            }

            public string GetMetadata(string metadataName)
            {
                if (metadataName != "TargetPath")
                    return null;
                var fi = new FileInfo(_resourceFilePath);
                return fi.Directory.Name + "\\" + fi.Name;
            }

            public void RemoveMetadata(string metadataName)
            {
                throw new NotImplementedException();
            }

            public void SetMetadata(string metadataName, string metadataValue)
            {
                throw new NotImplementedException();
            }
        }
    }
}
