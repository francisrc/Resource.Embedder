using System;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.Core.GeneratedCode
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

        internal static void Dettach()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve -= AssemblyResolve;
        }

        /// <summary>
        /// Attach to resolve satellite assemblies from embedded resources.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName requestedAssemblyName;
            try
            {
                // validate user input
                // needed e.g. when Type.GetType is used as we are then part of the resolve chain
                requestedAssemblyName = new AssemblyName(args.Name);
            }
            catch (Exception e) when (e is ArgumentException || e is FileLoadException)
            {
                return null;
            }
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

            // resources have the same name as their belonging assembly, so find by name
            var parentName = requestedAssemblyName.Name.Substring(0, requestedAssemblyName.Name.Length - ".resources".Length);
            // I'd love to use linq here, but Cecil starts fucking up when I do (null reference exception on assembly.Write)
            // without a Linq query it works fine, though

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name == parentName)
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
            return requestedAssemblyName.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase);
        }

        private static Assembly LoadFromResource(AssemblyName requestedAssemblyName, Assembly requestingAssembly)
        {
            if (requestedAssemblyName == null || requestedAssemblyName.CultureInfo == null)
                return null; // without a concrete culture we cannot load a resource assembly

            // I haven't figured out how to add recursion to cecil (method cloner must know about the method itself already when copying it's instrutions)
            // so instead this is a loop with two possible exit points: localization found, or fallback route is depleted and we return null to let .Net locate the neutral resource
            while (true)
            {
                // requesting name in format: %assemblyname%.resources
                // rewrite to: %assemblyName%.%assemblyName%.%culture%.resources.dll
                //
                var baseName = requestedAssemblyName.Name.Substring(0, requestedAssemblyName.Name.Length - ".resources".Length);
                var name = string.Format("{0}.{1}.resources.dll", baseName, requestedAssemblyName.CultureInfo.Name);

                // by default for resources the requestingAssembly will be null
                var asm = requestingAssembly ?? FindMainAssembly(requestedAssemblyName);
                if (asm == null)
                {
                    // cannot find assembly from which to load
                    return null;
                }
                using (var stream = asm.GetManifestResourceStream(name))
                {
                    if (stream != null)
                    {
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        return Assembly.Load(bytes);
                    }
                }
                // did not find the specific resource yet
                // attempt to use the parent culture, this follows the .Net resource fallback system
                // e.g. if sub resource de-DE is not found, then .Parent will be "de", if that is not found parent will probably be default resource
                var fallback = requestedAssemblyName.CultureInfo.Parent.Name;
                if (string.IsNullOrEmpty(fallback))
                {
                    // is empty if no longer a parent
                    // return null so .Net can load the default resource
                    return null;
                }
                var alteredAssemblyName = requestedAssemblyName.FullName;
                alteredAssemblyName = alteredAssemblyName.Replace(string.Format("Culture={0}", requestedAssemblyName.CultureInfo.Name), string.Format("Culture={0}", fallback));

                requestedAssemblyName = new AssemblyName(alteredAssemblyName);
            }
        }

        #endregion Methods
    }
}