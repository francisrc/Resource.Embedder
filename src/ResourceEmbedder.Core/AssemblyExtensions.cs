using System.Reflection;

namespace ResourceEmbedder.Core
{
    public static class AssemblyExtensions
    {
        #region Methods

        /// <summary>
        /// Tries to resolve the assembly location as best as possible.
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        public static string GetLocation(this Assembly asm)
        {
            // codebase can resolve most pathes
            if (!string.IsNullOrEmpty(asm.CodeBase) && asm.CodeBase.StartsWith("file:///"))
                return asm.CodeBase.Substring("file:///".Length).Replace("/", "\\");

            if (!string.IsNullOrEmpty(asm.Location))
                return asm.Location;
            return "";
        }

        #endregion Methods
    }
}