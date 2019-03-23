using Mono.Cecil;
using ResourceEmbedder.Core.Extensions;
using ResourceEmbedder.Core.GeneratedCode;
using System;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.Core.Cecil
{
    /// <summary>
    /// Helpers for cecil usage.
    /// </summary>
    public static class CecilHelpers
    {
        /// <summary>
        /// When called will inject the <see cref="InjectedResourceLoader"/> type int the provided assembly.
        /// Then returns the <see cref="InjectedResourceLoader.Attach"/> method.
        /// </summary>
        /// <param name="definition">The assembly where the type should be added to.</param>
        /// <returns>A public, static method with no arguments that was added to the assembly.</returns>
        public static MethodDefinition InjectEmbeddedResourceLoader(AssemblyDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }
            var type = typeof(InjectedResourceLoader);
            var asm = Assembly.GetAssembly(type);
            var module = ModuleDefinition.ReadModule(asm.GetLocation());
            const string nameSpace = "ResourceEmbedderCompilerGenerated";
            const string className = "ResourceEmbedderILInjected";
            const string initMethod = "Attach";
            var existingType = definition.MainModule.GetTypes().FirstOrDefault(t => t.Namespace == nameSpace && t.Name == className);
            if (existingType != null)
            {
                // type already injected
                var existingMethod = existingType.Methods.FirstOrDefault(m => m.Name == initMethod);
                if (existingMethod == null)
                {
                    throw new MissingMethodException(string.Format("Found type {0}, but it does not have required method {1}. This indicates that you most likely created the method yourself. Please pick another class name.", className, initMethod));
                }
                return existingMethod;
            }
            var clonedType = TypeCloner.CloneTo(module.GetType(type.FullName), definition.MainModule, new[]
            {
                "FindMainAssembly",
                "LoadFromResource",
                "IsLocalizedAssembly",
                "AssemblyResolve",
                initMethod
            }, nameSpace, className);
            // add the type to the assembly.
            definition.MainModule.Types.Add(clonedType);
            // return the method
            return clonedType.Methods.First(m => m.Name == initMethod);
        }
    }
}
