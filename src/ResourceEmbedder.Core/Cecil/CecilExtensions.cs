using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace ResourceEmbedder.Core.Cecil
{
    /// <summary>
    /// Extensions of cecil types.
    /// </summary>
    public static class CecilExtensions
    {
        /// <summary>
        /// Returns the generic args of a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Collection<TypeReference> GetGenericInstanceArguments(this TypeReference type)
        {
            return ((GenericInstanceType)type).GenericArguments;
        }

        /// <summary>
        /// Returns a reference of a generic version of the provided base type.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] args)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                self.DeclaringType.MakeGenericInstanceType(args))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
            }

            return reference;
        }
    }
}
