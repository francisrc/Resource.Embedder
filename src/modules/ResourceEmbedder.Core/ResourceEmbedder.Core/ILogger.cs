namespace ResourceEmbedder.Core
{
	public interface ILogger
	{
		#region Methods

		/// <summary>
		/// Sets the indent to the specific level.
		/// Must be positive.
		/// </summary>
		/// <param name="level"></param>
		void Indent(int level);

		void Debug(string message, params object[] args);

		void Error(string message, params object[] args);

		void Info(string message, params object[] args);

		void Warning(string message, params object[] args);

		#endregion Methods
	}
}