namespace ResourceEmbedder.Core
{
    /// <summary>
    /// Abstract logger.
    /// </summary>
    public interface ILogger
    {
        #region Methods

        /// <summary>
        /// Debug type message.
        /// </summary>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Error type message.
        /// </summary>
        void Error(string message, params object[] args);

        /// <summary>
        /// Sets the indent to the specific level.
        /// Must be positive.
        /// </summary>
        /// <param name="level"></param>
        void Indent(int level);

        /// <summary>
        /// Info type message.
        /// </summary>
        void Info(string message, params object[] args);

        /// <summary>
        /// Warning type message.
        /// </summary>
        void Warning(string message, params object[] args);

        #endregion Methods
    }
}
