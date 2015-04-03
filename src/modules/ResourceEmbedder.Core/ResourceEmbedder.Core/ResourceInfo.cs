using System;

namespace ResourceEmbedder.Core
{
	/// <summary>
	/// Holds information regarding a resource to be embedded.
	/// </summary>
	public class ResourceInfo
	{
		#region Constructors

		public ResourceInfo(string fileToEmbedd, string relativePathInTargetAssembly)
		{
			if (string.IsNullOrEmpty(fileToEmbedd))
				throw new ArgumentException("fileToEmbedd");
			if (string.IsNullOrEmpty(relativePathInTargetAssembly))
				throw new ArgumentException("relativePathInTargetAssembly");

			FullPathOfFileToEmbedd = fileToEmbedd;
			RelativePathInAssembly = relativePathInTargetAssembly;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// The full path to the file that has to be embedded.
		/// </summary>
		public string FullPathOfFileToEmbedd { get; set; }

		/// <summary>
		/// The relative path in the target assembly.
		/// </summary>
		public string RelativePathInAssembly { get; set; }

		#endregion Properties
	}
}