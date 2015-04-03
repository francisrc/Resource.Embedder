namespace ResourceEmbedder.Core
{
	public interface ILogger
	{
		#region Methods

		void Debug(string message, params object[] args);

		void Error(string message, params object[] args);

		void Info(string message, params object[] args);

		void Warning(string message, params object[] args);

		#endregion Methods
	}
}