using Mono.Cecil;
using System;

namespace ResourceEmbedder.Core
{
	/// <summary>
	/// Helper that allows resource embedding and code injection.
	/// Target assembly will be saved on <see cref="IDisposable.Dispose"/>.
	/// </summary>
	public interface IModifyAssemblies : IDisposable
	{
		#region Properties

		/// <summary>
		/// Full path to the input assembly.
		/// </summary>
		string InputAssembly { get; }

		/// <summary>
		/// Full path to the output assembly location (may be the same as input).
		/// </summary>
		string OutputAssembly { get; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Call to embed the specific set of resources into the assembly.
		/// Call <see cref="Save"/> to persit changes to disk.
		/// </summary>
		/// <param name="resourceInfo"></param>
		/// <returns>True on success, false on error.</returns>
		bool EmbedResources(ResourceInfo[] resourceInfo);

		/// <summary>
		/// Call to inject code.
		/// This will hook up a module initializer (first to run when the specific assembly is loaded) and then allow you to add your own types, etc.
		/// by calling <see cref="func"/>. When you are done adding stuff, you must return the method that should be called by the module initializer.
		/// It must be: public, static and have no arguments.
		/// </summary>
		/// <param name="func"></param>
		/// <returns>True on success, false on error.</returns>
		bool InjectModuleInitializedCode(Func<AssemblyDefinition, MethodDefinition> func);

		/// <summary>
		/// Call to save changes.
		/// Also automatically called on dispose.
		/// </summary>
		void Save();

		#endregion Methods
	}
}