using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.Core.Cecil
{
    /// <summary>
    /// Helper class to clone one type into a new module.
    /// </summary>
    public class TypeCloner
    {
        private readonly ConstructorInfo _instructionConstructorInfo = typeof(Instruction).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(OpCode), typeof(object) }, null);
        private readonly TypeDefinition _sourceType;
        private readonly ModuleDefinition _targetModule;

        private TypeCloner(TypeDefinition sourceType, ModuleDefinition targetModule, string[] methodCloneOrder, string nameSpace = null, string className = null)
        {
            _targetModule = targetModule;
            _sourceType = sourceType;

            ClonedType = new TypeDefinition(nameSpace ?? _sourceType.Namespace, className ?? _sourceType.Name, _sourceType.Attributes, Resolve(_sourceType.BaseType));

            Assembly mscorlib;
            try
            {
                mscorlib = Assembly.Load("mscorlib");
            }
            catch (Exception e)
            {
                throw new DllNotFoundException("Failed to locate mscorlib", e);
            }
            IAssemblyResolver assemblyResolver = targetModule.AssemblyResolver;
            var msCoreLibDefinition = assemblyResolver.Resolve(new AssemblyNameReference("mscorlib", mscorlib.GetName().Version));
            var msCoreTypes = msCoreLibDefinition.MainModule.Types;
            var compilerGeneratedAttribute = msCoreTypes.First(x => x.Name == "CompilerGeneratedAttribute");
            var compilerGeneratedAttributeCtor = targetModule.ImportReference(compilerGeneratedAttribute.Methods.First(x => x.IsConstructor));
            ClonedType.CustomAttributes.Add(new CustomAttribute(compilerGeneratedAttributeCtor));
            CopyFields(_sourceType, ClonedType);

            // Cecil throws on assembly.Write if we have wrong order
            /*
             * E.g. Method GetKey() calls Metod GetKeyInternal()
             * then we must add GetKeyInternal() first and then GetKey() otherwise said exception occurs
             */
            foreach (var s in methodCloneOrder)
            {
                ClonedType.Methods.Add(CopyMethod(_sourceType.Methods.First(m => m.Name == s)));
            }
        }

        public TypeDefinition ClonedType { get; }

        #region Methods

        /// <summary>
        /// Clones the provided type definition from one assembly to the other.
        /// </summary>
        /// <param name="sourceType">The type to clone.</param>
        /// <param name="targetModule">The module where the type should be added to.</param>
        /// <param name="methodCloneOrder">Cecil crashes when methods are added in wrong order. You must manually sort your methods in their reverse execution order if they depend on each other. I'm sure that this problem can be solved, but for now this implementation is "good enough" for me.</param>
        /// <param name="nameSpace">The namespace to use. Leave null to use same namespace as in <see cref="sourceType"/></param>
        /// <param name="className">The classname to use. Leave null to use same classname as in <see cref="sourceType"/></param>
        /// <returns>The cloned type, already added to the targetModule.</returns>
        public static TypeDefinition CloneTo(TypeDefinition sourceType, ModuleDefinition targetModule, string[] methodCloneOrder, string nameSpace = null, string className = null)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            return new TypeCloner(sourceType, targetModule, methodCloneOrder, nameSpace, className).ClonedType;
        }

        private Instruction CloneInstruction(Instruction instruction, string fullyQualifiedPath)
        {
            var newInstruction = (Instruction)_instructionConstructorInfo.Invoke(new[] { instruction.OpCode, instruction.Operand });
            newInstruction.Operand = Import(instruction.Operand);
            return newInstruction;
        }

        private void CopyExceptionHandlers(MethodDefinition templateMethod, MethodDefinition newMethod)
        {
            if (!templateMethod.Body.HasExceptionHandlers)
            {
                return;
            }
            foreach (var exceptionHandler in templateMethod.Body.ExceptionHandlers)
            {
                var handler = new ExceptionHandler(exceptionHandler.HandlerType);
                var templateInstructions = templateMethod.Body.Instructions;
                var targetInstructions = newMethod.Body.Instructions;
                if (exceptionHandler.TryStart != null)
                {
                    handler.TryStart = targetInstructions[templateInstructions.IndexOf(exceptionHandler.TryStart)];
                }
                if (exceptionHandler.TryEnd != null)
                {
                    handler.TryEnd = targetInstructions[templateInstructions.IndexOf(exceptionHandler.TryEnd)];
                }
                if (exceptionHandler.HandlerStart != null)
                {
                    handler.HandlerStart = targetInstructions[templateInstructions.IndexOf(exceptionHandler.HandlerStart)];
                }
                if (exceptionHandler.HandlerEnd != null)
                {
                    handler.HandlerEnd = targetInstructions[templateInstructions.IndexOf(exceptionHandler.HandlerEnd)];
                }
                if (exceptionHandler.FilterStart != null)
                {
                    handler.FilterStart = targetInstructions[templateInstructions.IndexOf(exceptionHandler.FilterStart)];
                }
                if (exceptionHandler.CatchType != null)
                {
                    handler.CatchType = Resolve(exceptionHandler.CatchType);
                }
                newMethod.Body.ExceptionHandlers.Add(handler);
            }
        }

        private void CopyFields(TypeDefinition source, TypeDefinition target)
        {
            foreach (var field in source.Fields)
            {
                var newField = new FieldDefinition(field.Name, field.Attributes, Resolve(field.FieldType));
                target.Fields.Add(newField);
            }
        }

        private void CopyInstructions(MethodDefinition templateMethod, MethodDefinition newMethod)
        {
            var name = templateMethod.Module.FileName;
            foreach (var instruction in templateMethod.Body.Instructions)
            {
                var newInstruction = (Instruction)_instructionConstructorInfo.Invoke(new[] { instruction.OpCode, instruction.Operand });
                newInstruction.Operand = Import(instruction.Operand);
                newMethod.Body.Instructions.Add(newInstruction);
                var sequencePoint = templateMethod.DebugInformation.GetSequencePoint(instruction);
                if (sequencePoint != null)
                {
                    newMethod.DebugInformation.SequencePoints.Add(TranslateSequencePoint(newInstruction, sequencePoint, name));
                }
            }
        }

        private MethodDefinition CopyMethod(MethodDefinition templateMethod, bool makePrivate = false)
        {
            var attributes = templateMethod.Attributes;
            if (makePrivate)
            {
                attributes &= ~Mono.Cecil.MethodAttributes.Public;
                attributes |= ~Mono.Cecil.MethodAttributes.Private;
            }
            var returnType = Resolve(templateMethod.ReturnType);
            var newMethod = new MethodDefinition(templateMethod.Name, attributes, returnType)
            {
                IsPInvokeImpl = templateMethod.IsPInvokeImpl,
                IsPreserveSig = templateMethod.IsPreserveSig,
            };
            if (templateMethod.IsPInvokeImpl)
            {
                var moduleRef = _targetModule.ModuleReferences.FirstOrDefault(mr => mr.Name == templateMethod.PInvokeInfo.Module.Name);
                if (moduleRef == null)
                {
                    moduleRef = new ModuleReference(templateMethod.PInvokeInfo.Module.Name);
                    _targetModule.ModuleReferences.Add(moduleRef);
                }
                newMethod.PInvokeInfo = new PInvokeInfo(templateMethod.PInvokeInfo.Attributes, templateMethod.PInvokeInfo.EntryPoint, moduleRef);
            }

            if (templateMethod.Body != null)
            {
                newMethod.Body.InitLocals = templateMethod.Body.InitLocals;
                foreach (var variableDefinition in templateMethod.Body.Variables)
                {
                    var newVariableDefinition = new VariableDefinition(Resolve(variableDefinition.VariableType));
                    newMethod.Body.Variables.Add(newVariableDefinition);
                }
                CopyInstructions(templateMethod, newMethod);
                CopyExceptionHandlers(templateMethod, newMethod);
            }
            foreach (var parameterDefinition in templateMethod.Parameters)
            {
                var newParameterDefinition = new ParameterDefinition(Resolve(parameterDefinition.ParameterType));
                newParameterDefinition.Name = parameterDefinition.Name;
                newMethod.Parameters.Add(newParameterDefinition);
            }

            return newMethod;
        }

        private object Import(object operand)
        {
            var reference = operand as MethodReference;
            if (reference != null)
            {
                var methodReference = reference;
                if (methodReference.DeclaringType == _sourceType)
                {
                    var mr = ClonedType.Methods.FirstOrDefault(x => x.Name == methodReference.Name && x.Parameters.Count == methodReference.Parameters.Count);
                    if (mr == null)
                    {
                        return CopyMethod(methodReference.DeclaringType.Resolve().Methods
                                          .First(m => m.Name == methodReference.Name && m.Parameters.Count == methodReference.Parameters.Count),
                            methodReference.DeclaringType != _sourceType);
                    }
                    return mr;
                }
                if (methodReference.DeclaringType.IsGenericInstance)
                {
                    return _targetModule.ImportReference(methodReference.Resolve())
                        .MakeHostInstanceGeneric(methodReference.DeclaringType.GetGenericInstanceArguments().ToArray());
                }
                return _targetModule.ImportReference(methodReference.Resolve());
            }
            var typeReference = operand as TypeReference;
            if (typeReference != null)
            {
                return Resolve(typeReference);
            }
            var fieldReference = operand as FieldReference;
            if (fieldReference != null)
            {
                return ClonedType.Fields.FirstOrDefault(f => f.Name == fieldReference.Name) ?? operand;
            }
            return operand;
        }

        private TypeReference Resolve(TypeReference baseType)
        {
            var typeDefinition = baseType.Resolve();
            var typeReference = _targetModule.ImportReference(typeDefinition);
            if (baseType is ArrayType)
            {
                return new ArrayType(typeReference);
            }
            if (baseType.IsGenericInstance)
            {
                typeReference = typeReference.MakeGenericInstanceType(baseType.GetGenericInstanceArguments().ToArray());
            }
            return typeReference;
        }

        private SequencePoint TranslateSequencePoint(Instruction instruction, SequencePoint sequencePoint, string fullyQualifiedPath)
        {
            if (sequencePoint == null)
                return null;

            var document = new Document(Path.Combine(Path.GetDirectoryName(fullyQualifiedPath), Path.GetFileName(sequencePoint.Document.Url)))
            {
                Language = sequencePoint.Document.Language,
                LanguageVendor = sequencePoint.Document.LanguageVendor,
                Type = sequencePoint.Document.Type,
            };

            return new SequencePoint(instruction, document)
            {
                StartLine = sequencePoint.StartLine,
                StartColumn = sequencePoint.StartColumn,
                EndLine = sequencePoint.EndLine,
                EndColumn = sequencePoint.EndColumn,
            };
        }

        #endregion Methods
    }
}
