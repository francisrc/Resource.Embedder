using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;
using System.IO;

namespace ResourceEmbedder.Core.Cecil
{
	public class CecilBasedAssemblyModifier : IModifyAssemblies
	{
		#region Fields

		private readonly AssemblyDefinition _assemblyDefinition;
		private readonly IInjectCode _codeInjector;
		private readonly ILogger _logger;
		private readonly IEmbedResources _resourceEmbedder;
		private readonly bool _symbolsAreBeingRead;

		#endregion Fields

		#region Constructors

		public CecilBasedAssemblyModifier(ILogger logger, string inputAssembly, string outputAssembly, string[] searchDirectories = null)
		{
			if (logger == null)
			{
				throw new ArgumentNullException("logger");
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
			InputAssembly = Path.GetFullPath(inputAssembly);
			OutputAssembly = Path.GetFullPath(outputAssembly);

			var symbolPath = Path.ChangeExtension(inputAssembly, "pdb");
			_symbolsAreBeingRead = File.Exists(symbolPath);

			var resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(new FileInfo(inputAssembly).DirectoryName);

			if (searchDirectories != null)
				foreach (var dir in searchDirectories)
					resolver.AddSearchDirectory(dir);

			var rp = new ReaderParameters
			{
				ReadSymbols = _symbolsAreBeingRead,
				AssemblyResolver = resolver
			};

			_assemblyDefinition = AssemblyDefinition.ReadAssembly(inputAssembly, rp);
			_resourceEmbedder = new CecilBasedResourceEmbedder(logger);
			_codeInjector = new CecilBasedCodeInjector(logger);
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
			if (File.Exists(pdb))
			{
				_logger.Info("Rewritting pdb");
				// delete it just in case, as there have been issues before
				// (e.g. a file lock by ms build that Cecil silently smallows leaving us with the old pdb, thus non-debuggable ode)
				File.Delete(pdb);
			}
			_assemblyDefinition.Write(OutputAssembly, new WriterParameters
			{
				WriteSymbols = _symbolsAreBeingRead,
				SymbolWriterProvider = new PdbWriterProvider()
			});
		}

		#endregion Methods
	}
}