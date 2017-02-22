using System.IO;

namespace ResourceEmbedder.MsBuild
{
	public class FileHelper
	{
		#region Fields

		public const string FilePattern = "{0}.Resource.Embedder.embeddedCultures.temp";

		#endregion Fields

		#region Methods

		/// <summary>
		/// Returns a unique (but persistend) name for the given input file.
		/// </summary>
		/// <param name="inputAssembly"></param>
		/// <returns>Will be a full path to a file in the same directory as the input file.</returns>
		public static string GetUniqueTempFileName(string inputAssembly)
		{
			var fi = new FileInfo(inputAssembly);
			var name = fi.Name;

			return Path.Combine(fi.DirectoryName, string.Format(FilePattern, name));
		}

		#endregion Methods
	}
}