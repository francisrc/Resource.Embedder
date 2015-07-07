using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.MsBuild
{
	/// <summary>
	/// Since <see cref="SatelliteAssemblyEmbedderTask"/> only embedds the files,
	/// but doesn't prevent them from getting deployed we will have to do so manually.
	/// </summary>
	public class SatelliteAssemblyCleanupTask : MsBuildTask
	{
		#region Methods

		public override bool Execute()
		{
			var logger = new MSBuildBasedLogger(BuildEngine, "ResourceEmbedder");
			if (SignAssembly)
			{
				// TODO: check required steps to add this feature
				logger.Error("Signed assemblies have not been implemented yet.");
				return false;
			}
			if (!AssertSetup(logger))
			{
				return false;
			}

			var watch = new Stopwatch();
			watch.Start();
			logger.Info("Beginning resource cleanup.");
			logger.Indent(1);

			string outputAssembly = Path.Combine(ProjectDirectory, AssemblyPath);
			var workingDir = new FileInfo(outputAssembly).DirectoryName;
			logger.Info("WorkingDir (used for cleanup of resources): " + workingDir);
			logger.Info("Scanning output assembly '{0}' for embedded localizations", outputAssembly);

			// detect which cultures have been embedded
			var embeddedCultures = GetEmbeddedCultures(outputAssembly).ToList();
			// assembly name may be relative path + name of assembly, e.g. ..\output.exe
			// we need only the name for -> %name%.resources.dll
			var resourceName = Path.GetFileNameWithoutExtension(outputAssembly) + ".resources.dll";

			foreach (var ci in embeddedCultures)
			{
				var resourceFile = Path.Combine(workingDir, ci.Name, resourceName);
				if (File.Exists(resourceFile))
				{
					File.Delete(resourceFile);
					// check whether that was the last file of the specific language, if so -> delete the directory
					var dir = new FileInfo(resourceFile).DirectoryName;
					if (Directory.GetFileSystemEntries(dir).Length == 0)
					{
						// empty dir -> we just deleted the last resource from it, so delete it as well
						Directory.Delete(dir);
					}
					var message = string.Format("Deleted resource file '{0}' as it was embedded into the target.", Path.Combine(ci.Name, resourceName));
					logger.Info(message);
				}
			}

			watch.Stop();
			logger.Info("Finished cleanup in {0}ms", watch.ElapsedMilliseconds);
			return true;
		}

		/// <summary>
		/// For a given input file will find all embedded resources that have been embedded by <see cref="SatelliteAssemblyEmbedderTask"/>.
		/// </summary>
		/// <param name="outputAssembly"></param>
		/// <returns></returns>
		private static IEnumerable<CultureInfo> GetEmbeddedCultures(string outputAssembly)
		{
			var asm = Assembly.ReflectionOnlyLoadFrom(outputAssembly);
			var names = asm.GetManifestResourceNames();
			var fileName = Path.GetFileNameWithoutExtension(outputAssembly);
			// looking for %fileNamr%.%culture%.resources.dll
			var search = fileName + ".";
			const string end = ".resources.dll";
			var potentialCultures = names.Where(n => n.StartsWith(fileName) && n.EndsWith(end));
			foreach (var ci in potentialCultures)
			{
				var cultureName = ci.Substring(search.Length);
				cultureName = cultureName.Substring(0, cultureName.Length - end.Length);
				CultureInfo culture;
				try
				{
					culture = CultureInfo.GetCultureInfo(cultureName);
				}
				catch (Exception)
				{
					// possible that user has embedded something with same name pattern, just ignore it
					continue;
				}
				yield return culture;
			}
		}

		#endregion Methods
	}
}