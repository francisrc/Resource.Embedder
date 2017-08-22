using FluentAssertions;
using Microsoft.Build.Framework;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace ResourceEmbedder.MsBuild.Tests
{
	[TestFixture]
	public class FullMsBuildTests
	{
		#region Methods

		[Test]
		public void MsBuildBasedEmbedding()
		{
			const string msBuild = "MsBuildBasedInjected.exe";
			if (File.Exists(msBuild))
			{
				File.Delete(msBuild);
			}
			const string de = "de\\MsBuildBasedInjected.resources.dll";
			File.Copy("de\\WpfTest.resources.dll", de, true);
			const string fr = "fr\\MsBuildBasedInjected.resources.dll";
			File.Copy("fr\\WpfTest.resources.dll", fr, true);
			File.Copy("WpfTest.exe", msBuild);
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

			var p = Process.Start(msBuild, "/testFullyProcessed");
			p.WaitForExit(2000).Should().BeTrue();
			p.ExitCode.Should().Be(0);
			File.Delete(de);
			File.Delete(fr);
		}

		[Test]
		public void MsBuildBasedEmbeddingWithSymbols()
		{
			const string msBuild = "MsBuildBasedInjected.exe";
			if (File.Exists(msBuild))
			{
				File.Delete(msBuild);
			}
			const string de = "de\\MsBuildBasedInjected.resources.dll";
			File.Copy("de\\WpfTest.resources.dll", de, true);
			const string fr = "fr\\MsBuildBasedInjected.resources.dll";
			File.Copy("fr\\WpfTest.resources.dll", fr, true);
			File.Copy("WpfTest.exe", msBuild);

			File.Copy("WpfTest.pdb", Path.ChangeExtension(msBuild, "pdb"), true);
			File.Exists(Path.ChangeExtension(msBuild, "pdb")).Should().BeTrue("because we just copied it");

			var fakeEngine = NSubstitute.Substitute.For<IBuildEngine>();
			var task = new SatelliteAssemblyEmbedderTask
			{
				ProjectDirectory = ".",
				AssemblyPath = msBuild,
				TargetPath = Path.GetFullPath(msBuild),
				BuildEngine = fakeEngine,
				References = ".",
				DebugSymbols = true,
				DebugType = "full"
			};
			task.Execute().Should().BeTrue();

			// pdb should not exist because we disable debug symbols
			File.Exists(Path.ChangeExtension(msBuild, "pdb")).Should().BeTrue("because we told the task to rewrite pdb");

			File.Delete(de);
			File.Delete(fr);
		}

		[Test]
		public void MsBuildBasedEmbeddingWithMissingSymbolsShouldThrow()
		{
			const string msBuild = "MsBuildBasedInjected.exe";
			if (File.Exists(msBuild))
			{
				File.Delete(msBuild);
			}
			const string de = "de\\MsBuildBasedInjected.resources.dll";
			File.Copy("de\\WpfTest.resources.dll", de, true);
			const string fr = "fr\\MsBuildBasedInjected.resources.dll";
			File.Copy("fr\\WpfTest.resources.dll", fr, true);
			File.Copy("WpfTest.exe", msBuild);
			if (File.Exists(Path.ChangeExtension(msBuild, "pdb")))
				File.Delete(Path.ChangeExtension(msBuild, "pdb"));
			var fakeEngine = NSubstitute.Substitute.For<IBuildEngine>();
			var task = new SatelliteAssemblyEmbedderTask
			{
				ProjectDirectory = ".",
				AssemblyPath = msBuild,
				TargetPath = Path.GetFullPath(msBuild),
				BuildEngine = fakeEngine,
				References = ".",
				DebugSymbols = true,
				DebugType = "full"
			};
			new Action(() => task.Execute()).ShouldThrow<NotSupportedException>("because pdb file is required but we deleted it manually before task executed");

			File.Delete(de);
			File.Delete(fr);
		}

		[Test]
		public void MsBuildBasedEmbeddingWithoutSymbols()
		{
			const string msBuild = "MsBuildBasedInjected.exe";
			if (File.Exists(msBuild))
			{
				File.Delete(msBuild);
			}
			const string de = "de\\MsBuildBasedInjected.resources.dll";
			File.Copy("de\\WpfTest.resources.dll", de, true);
			const string fr = "fr\\MsBuildBasedInjected.resources.dll";
			File.Copy("fr\\WpfTest.resources.dll", fr, true);
			File.Copy("WpfTest.exe", msBuild);
			if (File.Exists(Path.ChangeExtension(msBuild, "pdb")))
				File.Delete(Path.ChangeExtension(msBuild, "pdb"));
			var fakeEngine = NSubstitute.Substitute.For<IBuildEngine>();
			var task = new SatelliteAssemblyEmbedderTask
			{
				ProjectDirectory = ".",
				AssemblyPath = msBuild,
				TargetPath = Path.GetFullPath(msBuild),
				BuildEngine = fakeEngine,
				References = ".",
				DebugSymbols = false
			};
			task.Execute().Should().BeTrue();

			// pdb should not exist because we disable debug symbols
			File.Exists(Path.ChangeExtension(msBuild, "pdb")).Should().BeFalse();


			File.Delete(de);
			File.Delete(fr);
		}

		[Test]
		public void MsBuildBasedEmbeddingAndCleanup()
		{
			const string msBuild = "MsBuildBasedInjected.exe";
			if (File.Exists(msBuild))
			{
				File.Delete(msBuild);
			}
			const string de = "de\\MsBuildBasedInjected.resources.dll";
			File.Copy("de\\WpfTest.resources.dll", de, true);
			// de is german in generall, de-DE is german specific to germany -> if someone has set his localitation to German (Germany) he would get de-DE, if he sets it to e.g. German (Austria) he should get "de"
			// let's ensure that both levels of localization are correctly embedded
			const string deDe = "de-DE\\MsBuildBasedInjected.resources.dll";
			File.Copy("de-DE\\WpfTest.resources.dll", deDe, true);
			const string fr = "fr\\MsBuildBasedInjected.resources.dll";
			File.Copy("fr\\WpfTest.resources.dll", fr, true);
			File.Copy("WpfTest.exe", msBuild);

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

		#endregion Methods
	}
}