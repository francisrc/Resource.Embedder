using Mono.Cecil;
using System;
using System.IO;

namespace ResourceEmbedder.Core.Cecil
{
	/// <summary>
	/// Implementation that uses Cecil to embedd resources into .Net assemblies.
	/// </summary>
	public class CecilBasedFileEmbedder : IEmbedFiles
	{
		#region Constructors

		public CecilBasedFileEmbedder(ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			Logger = logger;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// The logger used during the embedding.
		/// </summary>
		public ILogger Logger { get; private set; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Call to embedd the provided set of resources into the specific assembly.
		/// Uses the <see cref="Logger"/> to issue log messages.
		/// </summary>
		/// <param name="inputAssembly">The assembly where the resources should be embedded in.</param>
		/// <param name="outputAssembly">The output path where the result should be stored. May be equal to <see cref="inputAssembly"/>.</param>
		/// <param name="resourcesToEmbedd"></param>
		/// <returns></returns>
		public bool EmbedResources(string inputAssembly, string outputAssembly, ResourceInfo[] resourcesToEmbedd)
		{
			if (string.IsNullOrEmpty(inputAssembly) || string.IsNullOrEmpty(outputAssembly) || resourcesToEmbedd == null)
			{
				throw new ArgumentException();
			}
			if (resourcesToEmbedd.Length == 0)
			{
				throw new ArgumentException("No resources to embed");
			}
			try
			{
				var assemblyDef = AssemblyDefinition.ReadAssembly(inputAssembly);
				Logger.Info("Embedding {0} files into {1}", resourcesToEmbedd.Length, outputAssembly);
				foreach (var res in resourcesToEmbedd)
				{
					if (!File.Exists(res.FullPathOfFileToEmbedd))
					{
						Logger.Error("Could not locate file '{0}' for embedding.", res.FullPathOfFileToEmbedd);
						return false;
					}
					try
					{
						var bytes = File.ReadAllBytes(res.FullPathOfFileToEmbedd);
						assemblyDef.MainModule.Resources.Add(new EmbeddedResource(res.RelativePathInAssembly, ManifestResourceAttributes.Private, bytes));
					}
					catch (Exception ex)
					{
						Logger.Error("Embedding task failed for resource {0}. Could not embedd into {1}. {2}", res.FullPathOfFileToEmbedd, outputAssembly, ex.Message);
						return false;
					}
				}
				Logger.Info("Finalizing output assembly {0}.", outputAssembly);
				assemblyDef.Write(outputAssembly);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
				return false;
			}
			return true;
		}

		#endregion Methods
	}
}