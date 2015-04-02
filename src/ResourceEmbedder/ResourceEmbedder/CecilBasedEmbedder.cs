using Mono.Cecil;
using System;
using System.IO;

namespace ResourceEmbedder
{
	/// <summary>
	/// Implementation that uses Cecil to embedd resources into .Net assemblies.
	/// </summary>
	public class CecilBasedEmbedder : IEmbeddFiles
	{
		public CecilBasedEmbedder(ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			Logger = logger;
		}

		/// <summary>
		/// The logger used during the embedding.
		/// </summary>
		public ILogger Logger { get; private set; }

		/// <summary>
		/// Call to embedd the provided set of resources into the specific assembly.
		/// Uses the <see cref="Logger"/> to issue log messages.
		/// </summary>
		/// <param name="targetAssembly">The assembly where the resources should be embedded in.</param>
		/// <param name="resourcesToEmbedd"></param>
		/// <returns></returns>
		public bool EmbedResources(string targetAssembly, ResourceInfo[] resourcesToEmbedd)
		{
			var assemblyDef = AssemblyDefinition.ReadAssembly(targetAssembly);
			Logger.LogInfo("Embedding {0} files into {1}", resourcesToEmbedd.Length, targetAssembly);
			foreach (var res in resourcesToEmbedd)
			{
				if (!File.Exists(res.FullPathOfFileToEmbedd))
				{
					Logger.LogError("Could not locate file '{0}' for embedding.", res.FullPathOfFileToEmbedd);
					return false;
				}
				try
				{
					var bytes = File.ReadAllBytes(res.FullPathOfFileToEmbedd);
					assemblyDef.MainModule.Resources.Add(new EmbeddedResource(res.RelativePathInAssembly, ManifestResourceAttributes.Private, bytes));
				}
				catch (Exception ex)
				{
					Logger.LogError("Embedding task failed for resource {0}. Could not embedd into {1}. {2}", res.FullPathOfFileToEmbedd, targetAssembly, ex.Message);
					return false;
				}
				Logger.LogInfo("Finalizing output assembly {0}.", targetAssembly);
				assemblyDef.Write(targetAssembly);
			}
			return true;
		}
	}
}