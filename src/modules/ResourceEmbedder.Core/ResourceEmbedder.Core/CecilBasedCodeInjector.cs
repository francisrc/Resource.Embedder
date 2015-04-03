using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace ResourceEmbedder.Core
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
		public bool Inject(string targetAssembly, Func<AssemblyDefinition, MethodDefinition> methodToCall)
		{
			try
			{
				// first create a .cctor in the default module this is called before any other code. http://einaregilsson.com/module-initializers-in-csharp/
				var asm = AssemblyDefinition.ReadAssembly(targetAssembly);
				var def = FindOrCreateCctor(asm.MainModule.Types.FirstOrDefault(t => t.Name == "<Module>"));
				var body = def.Body;
				body.SimplifyMacros();
				// don't fully replace the code, always possible that user already uses code in here (e.g. when using Fody/Costura)
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
				asm.Write(targetAssembly);
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
		/// <param name="targetType"></param>
		/// <returns></returns>
		private static MethodDefinition FindOrCreateCctor(TypeDefinition targetType)
		{
			var cctor = targetType.Methods.FirstOrDefault(x => x.Name == ".cctor");
			if (cctor == null)
			{
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