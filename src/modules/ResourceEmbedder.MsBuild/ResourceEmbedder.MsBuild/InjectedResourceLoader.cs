using System;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.MsBuild
{
	/// <summary>
	/// Code that is injected into target assemblies.
	/// Upon request for localized assemblies this will resolve and load the embedded resources.
	/// </summary>
	public static class InjectedResourceLoader
	{
		#region Methods

		public static void Attach()
		{
			var currentDomain = AppDomain.CurrentDomain;
			currentDomain.AssemblyResolve += AssemblyResolve;
		}

		internal static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var requestedAssemblyName = new AssemblyName(args.Name);
			if (!IsLocalizedAssembly(requestedAssemblyName))
			{
				return null;
			}
			return LoadFromResource(requestedAssemblyName, args.RequestingAssembly);
		}

		private static Assembly FindMainAssembly(AssemblyName requestedAssemblyName)
		{
			var mainName = requestedAssemblyName.Name;
			mainName = mainName.Substring(0, mainName.Length - ".resources".Length);
			return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == mainName);
		}

		/// <summary>
		/// Checks whether the requested assembly is a satellite assembly or not.
		/// </summary>
		/// <param name="requestedAssemblyName"></param>
		/// <returns></returns>
		private static bool IsLocalizedAssembly(AssemblyName requestedAssemblyName)
		{
			// only *.resources.dll files are satellite assemblies
			if (requestedAssemblyName.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase))
			{
				if (requestedAssemblyName.CultureName != "neutral")
				{
					return true;
				}
			}
			return false;
		}

		private static Assembly LoadFromResource(AssemblyName requestedAssemblyName, Assembly requestingAssembly)
		{
			// requesting name in format: %assemblyname%.resources
			// rewrite to: %%assemblyname%.%culture%.resources.dll
			//
			var baseName = requestedAssemblyName.Name.Substring(0, requestedAssemblyName.Name.Length - ".resources".Length);
			var name = string.Format("{0}.{1}.resources.dll", baseName, requestedAssemblyName.CultureName);

			// by default for resources the requestingAssembly will be null
			var asm = requestingAssembly ?? FindMainAssembly(requestedAssemblyName);
			if (asm == null)
			{
				// cannot find assembly from which to load
				return null;
			}
			using (var stream = asm.GetManifestResourceStream(name))
			{
				if (stream == null)
				{
					// not found
					return null;
				}
				var bytes = new byte[stream.Length];
				stream.Read(bytes, 0, bytes.Length);
				return Assembly.Load(bytes);
			}
		}

		#endregion Methods
	}
}