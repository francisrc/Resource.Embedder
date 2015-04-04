using Microsoft.Build.Framework;
using ResourceEmbedder.Core;
using ResourceEmbedder.Core.Cecil;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace ResourceEmbedder.MsBuild
{
	/// <summary>
	/// Task to embed files into an existing .Net assembly.
	/// </summary>
	public class EmbedderTask : Microsoft.Build.Utilities.Task
	{
		#region Properties

		[Required]
		public string AssemblyPath { set; get; }

		public string KeyFilePath { get; set; }

		[Required]
		public string ProjectDirectory { get; set; }

		public bool SignAssembly { get; set; }

		#endregion Properties

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
			var sw = new Stopwatch();
			logger.Info("Beginning resource embedding.");

			IEmbedFiles embedder = new CecilBasedEmbedder(logger);
			string inputAssembly = Path.Combine(ProjectDirectory, AssemblyPath);
			string outputAssembly = inputAssembly;
			var assembliesToEmbed = new List<ResourceInfo>();
			var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
			var inputAssemblyName = new FileInfo(inputAssembly).Name;
			// remove extension (.exe, .dll)
			inputAssemblyName = inputAssemblyName.Substring(0, inputAssemblyName.LastIndexOf('.'));
			foreach (var ci in cultures)
			{
				// check if culture satellite assembly exists, if so embed
				var ciPath = Path.Combine(ProjectDirectory, ci.TwoLetterISOLanguageName, string.Format("{0}.resources.dll", inputAssemblyName));
				if (File.Exists(ciPath))
				{
					logger.Debug("Embedding culture: {0}", ci.TwoLetterISOLanguageName);
					assembliesToEmbed.Add(new ResourceInfo(ciPath, string.Format("{0}.resources.dll", ci)));
				}
			}
			var r = embedder.EmbedResources(inputAssembly, outputAssembly, assembliesToEmbed.ToArray());
			sw.Stop();
			logger.Info("Finished embedding in {0}ms", sw.ElapsedMilliseconds);
			return r;
		}

		private bool AssertSetup(Core.ILogger logger)
		{
			if (!Directory.Exists(ProjectDirectory))
			{
				logger.Error("Project directory '{0}' does not exist.", ProjectDirectory);
				return false;
			}
			var asm = Path.Combine(ProjectDirectory, AssemblyPath);
			if (!File.Exists(asm))
			{
				logger.Error("Assembly '{0}' not found", asm);
				return false;
			}
			return true;
		}

		#endregion Methods
	}
}