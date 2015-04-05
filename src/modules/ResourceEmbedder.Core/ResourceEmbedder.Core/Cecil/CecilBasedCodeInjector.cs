using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;
using System;
using System.IO;
using System.Linq;

namespace ResourceEmbedder.Core.Cecil
{
	public class CecilBasedCodeInjector : IInjectCode
	{
		#region Constructors

		public CecilBasedCodeInjector(ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			Logger = logger;
		}

		#endregion Constructors

		#region Properties

		public ILogger Logger { get; private set; }

		#endregion Properties

		#region Methods

		/// <see cref="IInjectCode.Inject"/>
		public bool Inject(string inputAssembly, string outputAssembly, Func<AssemblyDefinition, MethodDefinition> methodToCall)
		{
			if (string.IsNullOrEmpty(inputAssembly) || string.IsNullOrEmpty(outputAssembly) || methodToCall == null)
			{
				throw new ArgumentNullException();
			}
			if (!File.Exists(inputAssembly))
			{
				throw new FileNotFoundException(inputAssembly);
			}
			try
			{
				var symbolPath = Path.ChangeExtension(inputAssembly, "pdb");
				var symbolsAreBeingRead = File.Exists(symbolPath);
				var asm = AssemblyDefinition.ReadAssembly(inputAssembly, new ReaderParameters { ReadSymbols = symbolsAreBeingRead });
				// first create a .cctor in the default module this is called before any other code. http://einaregilsson.com/module-initializers-in-csharp/
				var moduleInitializerMethod = FindOrCreateCctor(asm.MainModule);
				var body = moduleInitializerMethod.Body;
				body.SimplifyMacros();

				// don't fully replace the code, always possible that user already uses code in here (e.g. when using Fody/Costura in conjunction with our code)
				// instead, inject our invoke before every return statement
				var returnPoints = body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
				foreach (var returnPoint in returnPoints)
				{
					var idx = body.Instructions.IndexOf(returnPoint);

					var m = methodToCall(asm);
					AssertElligibilityForModuleInitializer(m);

					body.Instructions.Insert(idx, Instruction.Create(OpCodes.Call, m));
				}
				body.OptimizeMacros();
				var pdb = Path.ChangeExtension(outputAssembly, "pdb");
				if (File.Exists(pdb))
				{
					Logger.Info("Rewritting pdb");
					// delete it just in case, as there have been issues before
					// (e.g. a file lock by ms build that Cecil silently smallows leaving us with the old pdb, thus non-debuggable ode)
					File.Delete(pdb);
				}
				asm.Write(outputAssembly, new WriterParameters
				{
					WriteSymbols = symbolsAreBeingRead,
					SymbolWriterProvider = new PdbWriterProvider()
				});
			}
			catch (Exception ex)
			{
				Logger.Error("Failed injecting code. {0}", ex.Message);
				return false;
			}
			return true;
		}

		private static void AssertElligibilityForModuleInitializer(MethodDefinition m)
		{
			if (m == null)
			{
				throw new NullReferenceException("method to call may not be null");
			}
			if (!m.IsStatic)
			{
				throw new NotSupportedException("Method must be static");
			}
			if (!m.IsPublic)
			{
				throw new NotSupportedException("Method must be public");
			}
			if (m.Parameters.Count != 0)
			{
				throw new NotSupportedException("Method must have no arguments");
			}
		}

		/// <summary>
		/// Creates a new module initializer if none exists, else returns the existing.
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		private static MethodDefinition FindOrCreateCctor(ModuleDefinition module)
		{
			var targetType = module.Types.FirstOrDefault(t => t.Name == "<Module>");
			if (targetType == null)
			{
				throw new MissingFieldException("missing required type <Module> in assembly!");
			}
			var cctor = targetType.Methods.FirstOrDefault(x => x.Name == ".cctor");
			if (cctor == null)
			{
				// create new
				const MethodAttributes attributes = MethodAttributes.Private
													| MethodAttributes.HideBySig
													| MethodAttributes.Static
													| MethodAttributes.SpecialName
													| MethodAttributes.RTSpecialName;

				var _void = targetType.Module.Import(typeof(void));
				cctor = new MethodDefinition(".cctor", attributes, _void);
				targetType.Methods.Add(cctor);
				cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
			}
			return cctor;
		}

		#endregion Methods
	}
}