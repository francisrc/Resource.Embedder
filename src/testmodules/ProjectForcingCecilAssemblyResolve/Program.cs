using ProjectForcingCecilAssemblyResolve.Resources;
using ProjectWithEnum;
using System;
using System.Globalization;

namespace ProjectForcingCecilAssemblyResolve
{
	class Program
	{
		/// <summary>
		/// Test for https://github.com/MarcStan/Resource.Embedder/issues/5
		/// Cecil needs to resolve a TypeRef to write it properly when you have a const field with a TypeRef to an enum.
		/// So this project has a const ref to an enum from another assembly.
		/// This forces cecil to actually load the other assembly instead of just saying "I see the assembly name and assume it is valid" without actually checking for the assembly file.
		/// 
		/// The project with the enum is referenced as a project and set to "copy local: false". Then on successful build it is manually copied
		/// </summary>
		public const MyEnum EnumTest = MyEnum.One;

		static void Main(string[] args)
		{
			CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(args[0]);
			Console.WriteLine("Language: " + Translation.Language);

			Console.WriteLine("Const enum value is: " + EnumTest);
		}
	}
}
