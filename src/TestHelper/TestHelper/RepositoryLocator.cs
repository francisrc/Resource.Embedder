using System;
using System.IO;
using System.Reflection;

namespace TestHelper
{
	public class RepositoryLocator
	{
		#region Methods

		/// <summary>
		/// Returns the full path to the specific directory. Assumes that this assembly is currently place in the 'bin' folder parallel to the 'src'.
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public static string Locate(RepositoryDirectory dir)
		{
			var loc = Assembly.GetExecutingAssembly().CodeBase;
			// CodeBase format is stupid as hell, but we can't use Location property as shadow copying (NUnit, etc.) will put us into some temporary directories, while CodeBase always points to the original location the dll is run from
			if (loc.StartsWith("file:///"))
				loc = loc.Substring(8);
			loc = loc.Replace("/", "\\");

			// move up until we are in the root
			while (loc.Contains("\\") && !loc.EndsWith("\\bin"))
			{
				loc = loc.Substring(0, loc.LastIndexOf("\\", StringComparison.Ordinal));
			}
			if (!loc.Contains("\\"))
				return null;

			var scannerRoot = loc.Substring(0, loc.LastIndexOf("\\", StringComparison.Ordinal));

			switch (dir)
			{
				case RepositoryDirectory.SourceCode:
					return Path.Combine(scannerRoot, "src");
				case RepositoryDirectory.TestFiles:
					return Path.Combine(scannerRoot, "_TestFiles");
				default:
					throw new ArgumentOutOfRangeException("dir");
			}
		}

		#endregion Methods
	}
}