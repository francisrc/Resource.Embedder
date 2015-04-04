using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.Core.Cecil
{
	public class CecilHelpers
	{
		#region Methods

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
			var clonedType = new TypeCloner(module.GetType(type.FullName), definition.MainModule, new[]
			{
				"FindMainAssembly",
				"LoadFromResource",
				"IsLocalizedAssembly",
				"AssemblyResolve",
				"Attach"
			}, "ResourceEmbedderCompilerGenerated", "ResourceEmbedderILInjected").ClonedType;
			// add the type to the assembly.
			definition.MainModule.Types.Add(clonedType);
			// return the method
			return clonedType.Methods.First(m => m.Name == "Attach");
		}

		#endregion Methods
	}
}