using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.Core
{
	public class CecilHelper
	{
		#region Methods

		/// <summary>
		/// When called will inject the <see cref="InjectedResourceLoader"/> type int the provided assembly.
		/// Then returns the <see cref="InjectedResourceLoader.Attach"/> method.
		/// </summary>
		/// <param name="definition">The assembly where the type should be added to.</param>
		/// <returns>A public, static method with no arguments.</returns>
		public static MethodDefinition InjectEmbeddedResourceLoader(AssemblyDefinition definition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}
			var clone = typeof(InjectedResourceLoader);
			var asm = Assembly.GetAssembly(clone);
			var module = ModuleDefinition.ReadModule(asm.GetLocation());

			var clonedType = CloneType(module.GetType(clone.FullName));

			definition.MainModule.Types.Add(clonedType);

			// return the method
			return clonedType.Methods.First(m => m.Name == "Attach");
		}

		/// <summary>
		/// This method could certainly be extended.
		/// Take a look at https://github.com/gluck/il-repack for how to copy full types.
		/// For now only what was necessary has been added.
		/// </summary>
		/// <param name="typeToClone"></param>
		/// <returns></returns>
		private static TypeDefinition CloneType(TypeDefinition typeToClone)
		{
			throw new NotImplementedException("TODO: figure out how to clone a type");
		}

		#endregion Methods
	}
}