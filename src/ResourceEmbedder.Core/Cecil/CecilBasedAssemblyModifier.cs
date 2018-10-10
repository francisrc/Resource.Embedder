using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ResourceEmbedder.Core.Cecil
{
    public class CecilBasedAssemblyModifier : IModifyAssemblies
    {
        #region Fields

        private readonly AssemblyDefinition _assemblyDefinition;
        private readonly IInjectCode _codeInjector;
        private readonly ILogger _logger;
        private readonly StrongNameKeyPair _signingKey;
        private readonly IEmbedResources _resourceEmbedder;
        private readonly bool _symbolsAreBeingRead;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new modifier that can insert resources and code into an assembly.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="inputAssembly"></param>
        /// <param name="outputAssembly"></param>
        /// <param name="searchDirectories"></param>
        /// <param name="rewriteDebugSymbols">Determines whether debug symbols are read. If null the modifier will check for the existence of a .pdb file and if found will read it.
        /// If explicitely set to true and no pdb is found will cause an error.</param>
        /// <param name="signingKey">Optional signing key to be applied to the output assembly.</param>
        public CecilBasedAssemblyModifier(ILogger logger, string inputAssembly, string outputAssembly, string[] searchDirectories = null, bool? rewriteDebugSymbols = null, StrongNameKeyPair signingKey = null)
        {
            if (logger == null)
            {
                logger = new DummyLogger();
            }
            if (!File.Exists(inputAssembly))
            {
                throw new FileNotFoundException(inputAssembly);
            }
            if (string.IsNullOrEmpty(outputAssembly))
            {
                throw new ArgumentNullException("outputAssembly");
            }

            _logger = logger;
            _signingKey = signingKey;
            InputAssembly = Path.GetFullPath(inputAssembly);
            OutputAssembly = Path.GetFullPath(outputAssembly);

            var symbolPath = Path.ChangeExtension(inputAssembly, "pdb");
            var hasPdb = File.Exists(symbolPath);
            if (rewriteDebugSymbols.HasValue)
            {
                if (rewriteDebugSymbols.Value)
                {
                    _symbolsAreBeingRead = true;
                    if (!hasPdb)
                    {
                        throw new NotSupportedException($"User provided argument {nameof(rewriteDebugSymbols)} with value 'true' but could not locate required file '{symbolPath}'.");
                    }
                }
                else
                {
                    _symbolsAreBeingRead = false;
                }
            }
            else
            {
                // no value, default to reading when the file exists
                _symbolsAreBeingRead = hasPdb;
            }

            var rp = GetReaderParameters(inputAssembly, searchDirectories, _symbolsAreBeingRead);

            _assemblyDefinition = AssemblyDefinition.ReadAssembly(inputAssembly, rp);
            _resourceEmbedder = new CecilBasedResourceEmbedder(logger);
            _codeInjector = new CecilBasedCodeInjector(logger);
        }

        /// <summary>
        /// Helper to create the correct reader parameter construct based on the parameters.
        /// </summary>
        /// <param name="inputAssembly"></param>
        /// <param name="searchDirectories"></param>
        /// <param name="symbolsAreBeingRead"></param>
        /// <returns></returns>
        public static ReaderParameters GetReaderParameters(string inputAssembly, IEnumerable<string> searchDirectories, bool symbolsAreBeingRead)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(new FileInfo(inputAssembly).DirectoryName);

            if (searchDirectories != null)
                foreach (var dir in searchDirectories)
                    resolver.AddSearchDirectory(dir);

            var rp = new ReaderParameters
            {
                ReadSymbols = symbolsAreBeingRead,
                AssemblyResolver = resolver
            };
            return rp;
        }

        #endregion Constructors

        #region Properties

        public string InputAssembly { get; private set; }

        public string OutputAssembly { get; private set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            Save();
        }

        public bool EmbedResources(ResourceInfo[] resourceInfo)
        {
            return _resourceEmbedder.EmbedResources(_assemblyDefinition, resourceInfo);
        }

        public bool InjectModuleInitializedCode(Func<AssemblyDefinition, MethodDefinition> func)
        {
            return _codeInjector.Inject(_assemblyDefinition, func);
        }

        public void Save()
        {
            var pdb = Path.ChangeExtension(OutputAssembly, "pdb");
            var exists = File.Exists(pdb);
            if (exists && _symbolsAreBeingRead)
            {
                _logger.Info("Rewritting pdb");
            }
            if (exists)
            {
                // delete it just in case, as there have been issues before
                // (e.g. a file lock by ms build that Cecil silently swallows leaving us with the old pdb, thus non-debuggable ode)
                File.Delete(pdb);
            }
            _assemblyDefinition.Write(OutputAssembly, new WriterParameters
            {
                StrongNameKeyPair = _signingKey,
                WriteSymbols = _symbolsAreBeingRead,
                SymbolWriterProvider = _symbolsAreBeingRead ? new PdbWriterProvider() : null
            });
        }

        #endregion Methods
    }
}