using Mono.Cecil;
using System;

namespace ResourceEmbedder.Core
{
    public interface IInjectCode
    {
        #region Properties

        ILogger Logger { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// When called will inject a module initializer into it.
        /// It will then ask you to embed your further code via <see cref="methodToCall"/>.
        /// Once you have embedded your code, you must return the method that should be called from the module initializer.
        /// Your method must be: static, public and have no arguments.
        /// It will be the first to run when a module is loaded.
        /// </summary>
        /// <param name="assembly">The assembly on which to perform injection. Call <see cref="AssemblyDefinition.Write"/> to save changes.</param>
        /// <param name="methodToCall"></param>
        /// <returns></returns>
        bool Inject(AssemblyDefinition assembly, Func<AssemblyDefinition, MethodDefinition> methodToCall);

        #endregion Methods
    }
}