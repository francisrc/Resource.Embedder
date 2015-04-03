namespace ResourceEmbedder.Core
{
	/// <summary>
	/// Interface for a resource embedder.
	/// </summary>
	public interface IEmbedFiles
	{
		#region Properties

		/// <summary>
		/// The logger used during the embedding.
		/// </summary>
		ILogger Logger { get; }

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
		bool EmbedResources(string inputAssembly, string outputAssembly, ResourceInfo[] resourcesToEmbedd);

		#endregion Methods
	}
}