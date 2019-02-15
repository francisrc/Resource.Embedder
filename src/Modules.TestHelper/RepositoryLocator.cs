using System;
using System.IO;
using System.Reflection;

namespace Modules.TestHelper
{
    public static class RepositoryLocator
    {
        #region Methods

        /// <summary>
        /// Returns the full path to the specific directory. Assumes that this assembly is currently place in
        /// a subfolder of "src".
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
            while (loc.Contains("\\") && !loc.EndsWith("\\src"))
            {
                loc = loc.Substring(0, loc.LastIndexOf("\\", StringComparison.Ordinal));
            }
            if (!loc.Contains("\\"))
                return null;

            switch (dir)
            {
                case RepositoryDirectory.SourceCode:
                    return loc;
                case RepositoryDirectory.TestFiles:
                    return Path.Combine(loc, "testfiles");
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir));
            }
        }

        #endregion Methods
    }
}
