namespace ResourceEmbedder.Core
{
	/// <summary>
	/// Task to embed files into an existing .Net assembly.
	/// </summary>
	public class EmbedderTask : Microsoft.Build.Utilities.Task
	{
		#region Methods

		public override bool Execute()
		{
			var logger = new MSBuildBasedLogger(BuildEngine, "ResourceEmbedder");

			logger.LogInfo("Beginning resource embedding.");

			IEmbedFiles embedder = new CecilBasedEmbedder(logger);
			const string inputAssembly = @"TODO: load somehow";
			var assembliesToEmbedd = new ResourceInfo[0];
			return embedder.EmbedResources(inputAssembly, assembliesToEmbedd);
		}

		#endregion Methods
	}
}