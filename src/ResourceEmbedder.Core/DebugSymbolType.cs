namespace ResourceEmbedder.Core
{
    /// <summary>
    /// Wrapper around msbuild debug symbol options.
    /// </summary>
    public enum DebugSymbolType
    {
        /// <summary>
        /// No debug info
        /// </summary>
        None = 0,

        /// <summary>
        /// Full debug info (slow)
        /// </summary>
        Full = 1,

        /// <summary>
        /// Pdb only, default for .Net
        /// </summary>
        PdbOnly = 2,

        /// <summary>
        /// Default for .Net Core
        /// </summary>
        Portable = 3,

        /// <summary>
        /// Symbols embedded in output
        /// </summary>
        Embedded = 4
    }
}
