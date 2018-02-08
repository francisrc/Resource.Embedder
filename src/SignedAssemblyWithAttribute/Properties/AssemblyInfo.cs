using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("SignedAssemblyWithAttribute")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("SignedAssemblyWithAttribute")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("cf0f6e92-9632-47af-aa47-22ff7bbc7062")]

// adding signing key as attribute
[assembly: AssemblyKeyFile("Key.snk")]