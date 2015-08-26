namespace ResourceEmbedder.Core
{
	public class DummyLogger : ILogger
	{
		#region Methods

		public void Debug(string message, params object[] args)
		{
		}

		public void Error(string message, params object[] args)
		{
		}

		public void Indent(int level)
		{
		}

		public void Info(string message, params object[] args)
		{
		}

		public void Warning(string message, params object[] args)
		{
		}

		#endregion Methods
	}
}