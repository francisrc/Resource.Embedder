using System;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.Tests
{
	public class AssemblyHelper : IDisposable
	{
		#region Fields

		private Action _onDispose;

		#endregion Fields

		#region Constructors

		public AssemblyHelper(Assembly assembly, string location, Action onDispose)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException("location");

			if (!File.Exists(location))
				throw new FileNotFoundException("location");

			AssemblyLocation = location;
			_onDispose = onDispose;
			Assembly = assembly;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// The loaded assembly.
		/// </summary>
		public Assembly Assembly { get; private set; }

		public string AssemblyLocation { get; private set; }

		#endregion Properties

		#region Methods

		public void Dispose()
		{
			lock (_onDispose)
			{
				if (_onDispose != null)
				{
					_onDispose();
					_onDispose = null;
				}
			}
		}

		#endregion Methods
	}
}