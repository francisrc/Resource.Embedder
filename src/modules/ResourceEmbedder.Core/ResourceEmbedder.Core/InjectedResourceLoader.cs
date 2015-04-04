using System;
using System.Reflection;

namespace ResourceEmbedder.Core
{
	/// <summary>
	/// Code that is injected into target assemblies.
	/// Upon request for localized assemblies this will resolve and load the embedded resources.
	/// </summary>
	public static class InjectedResourceLoader
	{
		#region Methods

		/// <summary>
		/// Call once to attach the assembly resolve event.
		/// All embedded satellite assemblies will then be loaded.
		/// The convention is that each assembly stores it's own satellite assemblies as embedded resources.
		/// If the application name is WpfExe, then the resources are stored as WpfExe.de.resources.dll, WpfExe.fr.resources.dll, etc.
		/// and will be loaded by this code.
		/// </summary>
		public static void Attach()
		{
			var currentDomain = AppDomain.CurrentDomain;
			currentDomain.AssemblyResolve += AssemblyResolve;
		}

		/// <summary>
		/// Attach to resolve satellite assemblies from embedded resources.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		internal static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var requestedAssemblyName = new AssemblyName(args.Name);
			if (!IsLocalizedAssembly(requestedAssemblyName))
			{
				return null;
			}
			return LoadFromResource(requestedAssemblyName, args.RequestingAssembly);
		}

		/// <summary>
		/// Finds the main assembly for the specific resource.
		/// This requires that the resources name ends with .resources.
		/// </summary>
		/// <param name="requestedAssemblyName"></param>
		/// <returns></returns>
		private static Assembly FindMainAssembly(AssemblyName requestedAssemblyName)
		{
			if (requestedAssemblyName == null)
			{
				throw new ArgumentNullException("requestedAssemblyName");
			}
			if (!requestedAssemblyName.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new ArgumentException("Not a resource assembly");
			}
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			// I'd love to use linq here, but Cecil starts fucking up when I do (null reference exception on assembly.Write)
			// without a Linq query it works fine, though

			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var assembly in assemblies)
			{
				if (requestedAssemblyName.Name.StartsWith(assembly.GetName().Name))
				{
					return assembly;
				}
			}
			return null;
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
			// rewrite to: %assemblyName%.%assemblyName%.%culture%.resources.dll
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