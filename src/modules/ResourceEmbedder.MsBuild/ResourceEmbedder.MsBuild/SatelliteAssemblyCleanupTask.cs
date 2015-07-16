using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

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
			var logger = new MSBuildBasedLogger(BuildEngine, "ResourceEmbedder.Cleanup");
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
			var workingDir = new FileInfo(TargetPath).DirectoryName;
			//logger.Debug("WorkingDir (used for cleanup of resources): " + workingDir);
			//logger.Debug("Scanning output assembly '{0}' for embedded localizations", outputAssembly);

			// detect which cultures have been embedded
			var embeddedCultures = GetEmbeddedCultures(outputAssembly).ToList();
			// assembly name may be relative path + name of assembly, e.g. ..\output.exe
			// we need only the name for -> %name%.resources.dll
			var resourceName = Path.GetFileNameWithoutExtension(outputAssembly) + ".resources.dll";

			var embeddedResources = new List<string>();
			var emptyDirectories = new List<string>();
			foreach (var ci in embeddedCultures)
			{
				var resourceFile = Path.Combine(workingDir, ci.Name, resourceName);
				if (File.Exists(resourceFile))
				{
					embeddedResources.Add(ci.Name);
					File.Delete(resourceFile);
					// check whether that was the last file of the specific language, if so -> delete the directory
					var dir = new FileInfo(resourceFile).DirectoryName;
					if (Directory.GetFileSystemEntries(dir).Length == 0)
					{
						// empty dir -> we just deleted the last resource from it, so delete it as well
						try
						{
							Directory.Delete(dir);
							emptyDirectories.Add(ci.Name);
						}
						catch (Exception ex)
						{
							// happens e.g. if user has directory locked via another application without any files in the directory
							logger.Warning("Failed to delete resource directory '{0}': {1}", ci.Name, ex.Message);
						}
					}
				}
			}

			if (embeddedResources.Count == 1)
			{
				logger.Info("Deleted resource file '{0}' as it was embedded into the target.", embeddedResources[0]);
			}
			else if (embeddedResources.Count > 1)
			{
				logger.Info("Deleted resource files '{0}' as they where embedded into the target.", string.Join(", ", embeddedResources));
			}
			if (emptyDirectories.Count == 1)
			{
				logger.Info("Deleted resource directory '{0}' as it is empty.", emptyDirectories[0]);
			}
			else if (emptyDirectories.Count > 1)
			{
				logger.Info("Deleted resource directories '{0}' as they are empty.", string.Join(", ", emptyDirectories));
			}
			var notEmptyDirectories = embeddedResources.Except(emptyDirectories).ToList();
			if (notEmptyDirectories.Count == 1)
			{
				logger.Info("Resource directory '{0}' is not empty, thus will be kept.", notEmptyDirectories[0]);
			}
			else if (notEmptyDirectories.Count > 1)
			{
				logger.Info("Resource directories '{0}' are not empty, thus will be kept.", string.Join(", ", notEmptyDirectories));
			}
			watch.Stop();
			logger.Info("Finished cleanup in {0}ms", watch.ElapsedMilliseconds);
			return true;
		}

		/// <summary>
		/// Returns the list of culture names that have been embedded into the assembly by <see cref="SatelliteAssemblyEmbedderTask"/>.
		/// </summary>
		/// <param name="outputAssembly"></param>
		/// <returns></returns>
		private static string[] GetEmbeddedCultureNames(string outputAssembly)
		{
			// fix for issue#2
			// since we cannot pass parameters between tasks in MsBuild I originally loaded the assembly into appdomain to read all its resources
			// I also tried with a different appdomain but didn't get it to work
			// the simplest solution was to save a temp file from which the other task can read
			// to get a unique name we use the hash of the assembly
			var tempFile = FileHelper.GetUniqueTempFileName(outputAssembly);
			if (!File.Exists(tempFile))
			{
				// e.g. if processed assembly doesn't have localization
				return new string[0];
			}
			var cultures = File.ReadAllText(tempFile);
			File.Delete(tempFile);
			return cultures.Contains(";") ? cultures.Split(';') : new[] { cultures };
		}

		/// <summary>
		/// For a given input file will find all embedded resources that have been embedded by <see cref="SatelliteAssemblyEmbedderTask"/>.
		/// </summary>
		/// <param name="outputAssembly"></param>
		/// <returns></returns>
		private static IEnumerable<CultureInfo> GetEmbeddedCultures(string outputAssembly)
		{
			var names = GetEmbeddedCultureNames(outputAssembly);

			foreach (var ci in names)
			{
				CultureInfo culture;
				try
				{
					culture = CultureInfo.GetCultureInfo(ci);
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