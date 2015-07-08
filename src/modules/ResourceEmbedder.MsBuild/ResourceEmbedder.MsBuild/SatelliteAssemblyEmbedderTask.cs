using ResourceEmbedder.Core;
using ResourceEmbedder.Core.Cecil;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace ResourceEmbedder.MsBuild
{
	/// <summary>
	/// Task to embed satellite assemblies into an existing .Net assembly.
	/// Will also add code to the module initializer that will hook into AssemblyResolve event to load from emvbedded resources.
	/// </summary>
	public class SatelliteAssemblyEmbedderTask : MsBuildTask
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
			logger.Info("Beginning resource embedding.");
			logger.Indent(1);
			// run in object dir (=AssemblyPath) as we will run just after satellite assembly generated and ms build will then copy the output to target dir
			string inputAssembly = Path.Combine(ProjectDirectory, AssemblyPath);
			var workingDir = new FileInfo(inputAssembly).DirectoryName;
			logger.Info("WorkingDir (used for localization detection): " + workingDir);
			logger.Info("Input assembly: {0}", inputAssembly);

			var assembliesToEmbed = new List<ResourceInfo>();
			var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
			var inputAssemblyName = Path.GetFileNameWithoutExtension(inputAssembly);

			var usedCultures = new List<string>();
			foreach (var ci in cultures)
			{
				// check if culture satellite assembly exists, if so embed
				var ciPath = Path.Combine(workingDir, ci.Name, string.Format("{0}.resources.dll", inputAssemblyName));
				if (File.Exists(ciPath))
				{
					logger.Debug("Embedding culture: {0}", ci);
					usedCultures.Add(ci.Name);
					assembliesToEmbed.Add(new ResourceInfo(ciPath, string.Format("{0}.{1}.resources.dll", inputAssemblyName, ci)));
				}
			}
			if (assembliesToEmbed.Count == 0)
			{
				logger.Info("Nothing to embed! Skipping {0}", inputAssembly);
				return true;
			}

			using (IModifyAssemblies modifer = new CecilBasedAssemblyModifier(logger, inputAssembly, inputAssembly))
			{
				if (!modifer.EmbedResources(assembliesToEmbed.ToArray()))
				{
					logger.Error("Failed to embed resources into assembly: " + inputAssembly);
					return false;
				}
				if (!modifer.InjectModuleInitializedCode(CecilHelpers.InjectEmbeddedResourceLoader))
				{
					logger.Error("Failed to inject required code into assembly: " + inputAssembly);
					return false;
				}
			}
			watch.Stop();
			var tempFile = FileHelper.GetUniqueTempFileName(inputAssembly);
			File.WriteAllText(tempFile, string.Join(";", usedCultures));
			logger.Info("Finished embedding in {0}ms", watch.ElapsedMilliseconds);
			return true;
		}

		#endregion Methods
	}
}