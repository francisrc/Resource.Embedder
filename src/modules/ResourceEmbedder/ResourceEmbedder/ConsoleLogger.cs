using ResourceEmbedder.Core;
using System;

namespace ResourceEmbedder
{
	public class ConsoleLogger : ILogger
	{
		#region Methods

		public void LogError(string message, params object[] args)
		{
			LogColored(string.Format(message, args), ConsoleColor.Red);
		}

		public void LogInfo(string message, params object[] args)
		{
			LogColored(string.Format(message, args));
		}

		public void LogWarning(string message, params object[] args)
		{
			LogColored(string.Format(message, args), ConsoleColor.Yellow);
		}

		/// <summary>
		/// Outputs the specific message with the provided color.
		/// If no color provided leaves the systems default color.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="color">The color to use for printing. After printing the color will revert back to the previous one automatically.</param>
		private static void LogColored(string message, ConsoleColor? color = null)
		{
			// swap to new color, then revert back to old after printing message
			var old = Console.ForegroundColor;
			if (color.HasValue)
			{
				Console.ForegroundColor = color.Value;
			}
			Console.WriteLine(message);
			Console.ForegroundColor = old;
		}

		#endregion Methods
	}
}