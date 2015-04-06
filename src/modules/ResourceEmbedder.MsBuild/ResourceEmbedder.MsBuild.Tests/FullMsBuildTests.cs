using FluentAssertions;
using Microsoft.Build.Framework;
using NUnit.Framework;
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
			var fakeEngine = NSubstitute.Substitute.For<IBuildEngine>();
			var task = new SatelliteAssemblyEmbedderTask
			{
				ProjectDirectory = ".",
				AssemblyPath = msBuild,
				TargetPath = Path.GetFullPath("."),
				BuildEngine = fakeEngine
			};
			task.Execute().Should().BeTrue();

			var p = Process.Start(msBuild, "/testFullyProcessed");
			p.WaitForExit(2000).Should().BeTrue();
			p.ExitCode.Should().Be(0);
			File.Delete(de);
			File.Delete(fr);
		}

		#endregion Methods
	}
}