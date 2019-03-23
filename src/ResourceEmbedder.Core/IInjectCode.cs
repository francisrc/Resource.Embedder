using Mono.Cecil;
using System;

namespace ResourceEmbedder.Core
{
    /// <summary>
    /// Interface for code injection.
    /// </summary>
    public interface IInjectCode
    {
        /// <summary>
        /// The logger in use
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// When called will inject a module initializer into it.
        /// It will then ask you to embed your further code via methodToCall.
        /// Once you have embedded your code, you must return the method that should be called from the module initializer.
        /// Your method must be: static, public and have no arguments.
        /// It will be the first to run when a module is loaded.
        /// </summary>
        /// <param name="assembly">The assembly on which to perform injection.</param>
        /// <param name="methodToCall"></param>
        /// <returns></returns>
        bool Inject(AssemblyDefinition assembly, Func<AssemblyDefinition, MethodDefinition> methodToCall);
    }
}
