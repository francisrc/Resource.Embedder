using Microsoft.Build.Framework;
using System;
using ILogger = ResourceEmbedder.Core.ILogger;

namespace ResourceEmbedder.MsBuild
{
	public class MSBuildBasedLogger : ILogger
	{
		#region Fields

		private readonly IBuildEngine _buildEngine;
		private readonly string _sender;
		private int _indentLevel;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Creates a logger that uses the MS build engine to issue log statements.
		/// </summary>
		/// <param name="buildEngine"></param>
		/// <param name="sender">The sender name that will be used in the MS build log.</param>
		public MSBuildBasedLogger(IBuildEngine buildEngine, string sender)
		{
			if (buildEngine == null)
				throw new ArgumentNullException("buildEngine");
			if (sender == null)
				throw new ArgumentNullException("sender");

			_buildEngine = buildEngine;
			_sender = sender;
		}

		#endregion Constructors

		#region Methods

		public void Debug(string message, params object[] args)
		{
			Info(message, args);
		}

		public void Error(string message, params object[] args)
		{
			_buildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, Format(message, args), "", _sender));
		}

		private string Format(string message, object[] args)
		{
			return new string('\t', _indentLevel) + string.Format(message, args);
		}

		public void Info(string message, params object[] args)
		{
			_buildEngine.LogMessageEvent(new BuildMessageEventArgs(Format(message, args), "", _sender, MessageImportance.High));
		}

		public void Warning(string message, params object[] args)
		{
			_buildEngine.LogWarningEvent(new BuildWarningEventArgs("", "", "", 0, 0, 0, 0, Format(message, args), "", _sender));
		}

		#endregion Methods

		public void Indent(int level)
		{
			if (level < 0)
			{
				throw new ArgumentOutOfRangeException("level");
			}
			_indentLevel = level;
		}
	}
}