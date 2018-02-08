using System;
using System.IO;

namespace ResourceEmbedder.MsBuild
{
	public class WindowsSdkHelper
	{
		#region Methods

		/// <summary>
		/// Finds the corflags.exe which is located in one of many directories depeneding on installed software.
		/// </summary>
		/// <returns></returns>
		public static string FindCorFlagsExe()
		{
			return FindPathForWindowsSdk("CorFlags.exe");
		}

		private static string FindPathForWindowsSdk(string file)
		{
			string[] windowsSdkPaths =
			{
				@"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.1 Tools\",
				@"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\",
				@"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.2 Tools\",
				@"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\",
				@"Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.0 Tools\",
				@"Microsoft SDKs\Windows\v8.1A\bin\",
				@"Microsoft SDKs\Windows\v8.1\bin\NETFX 4.0 Tools\",
				@"Microsoft SDKs\Windows\v8.1\bin\",
				@"Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\",
				@"Microsoft SDKs\Windows\v8.0A\bin\",
				@"Microsoft SDKs\Windows\v8.0\bin\NETFX 4.0 Tools\",
				@"Microsoft SDKs\Windows\v8.0\bin\",
				@"Microsoft SDKs\Windows\v7.1A\bin\NETFX 4.0 Tools\",
				@"Microsoft SDKs\Windows\v7.1A\bin\",
				@"Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\",
				@"Microsoft SDKs\Windows\v7.0A\bin\",
				@"Microsoft SDKs\Windows\v6.1A\bin\",
				@"Microsoft SDKs\Windows\v6.0A\bin\",
				@"Microsoft SDKs\Windows\v6.0\bin\",
				@"Microsoft.NET\FrameworkSDK\bin"
			};

			foreach (var possiblePath in windowsSdkPaths)
			{
				string fullPath = string.Empty;

				// Check alternate program file paths as well as 64-bit versions.
				if (Environment.Is64BitProcess)
				{
					fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), possiblePath, "x64");
					if (Directory.Exists(fullPath))
					{
						var f = Path.Combine(fullPath, file);
						if (File.Exists(f))
							return f;
					}

					fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), possiblePath, "x64");
					if (Directory.Exists(fullPath))
					{
						var f = Path.Combine(fullPath, file);
						if (File.Exists(f))
							return f;
					}
				}

				fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), possiblePath);
				if (Directory.Exists(fullPath))
				{
					var f = Path.Combine(fullPath, file);
					if (File.Exists(f))
						return f;
				}

				fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), possiblePath);
				if (Directory.Exists(fullPath))
				{
					var f = Path.Combine(fullPath, file);
					if (File.Exists(f))
						return f;
				}
			}

			return null;
		}

		#endregion Methods
	}
}