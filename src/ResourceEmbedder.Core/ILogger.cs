namespace ResourceEmbedder.Core
{
    public interface ILogger
    {
        #region Methods

        void Debug(string message, params object[] args);

        void Error(string message, params object[] args);

        /// <summary>
        /// Sets the indent to the specific level.
        /// Must be positive.
        /// </summary>
        /// <param name="level"></param>
        void Indent(int level);

        void Info(string message, params object[] args);

        void Warning(string message, params object[] args);

        #endregion Methods
    }
}