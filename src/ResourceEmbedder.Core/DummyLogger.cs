namespace ResourceEmbedder.Core
{
    /// <summary>
    /// Logger that logs to /dev/null
    /// </summary>
    public class DummyLogger : ILogger
    {
        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Indent(int level)
        {
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Warning(string message, params object[] args)
        {
        }
    }
}
