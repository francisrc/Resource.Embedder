using Mono.Cecil;

namespace ResourceEmbedder.Core
{
    /// <summary>
    /// Interface for a resource embedder.
    /// </summary>
    public interface IEmbedResources
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
        /// <param name="assembly">The assembly on which to perform injection. Call <see cref="AssemblyDefinition.Write"/> to save changes.</param>
        /// <param name="resourcesToEmbedd"></param>
        /// <returns></returns>
        bool EmbedResources(AssemblyDefinition assembly, ResourceInfo[] resourcesToEmbedd);

        #endregion Methods
    }
}