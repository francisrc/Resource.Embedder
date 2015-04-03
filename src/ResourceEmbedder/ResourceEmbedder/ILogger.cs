namespace ResourceEmbedder
{
	public interface ILogger
	{
		#region Methods

		void LogError(string message, params object[] args);

		void LogInfo(string message, params object[] args);

		void LogWarning(string message, params object[] args);

		#endregion Methods
	}
}