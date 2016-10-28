using ResourceEmbedder.Core;
using ResourceEmbedder.Core.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace ResourceEmbedder
{
	class Program
	{
		#region Methods

		static void Main(string[] args)
		{
			if (args.Length < 3)
			{
				PrintUsageAndExit();
			}
			var logger = new ConsoleLogger();
			var input = args[0];
			if (!input.StartsWith("/input:"))
			{
				logger.Error("First argument must be /input:<input_assembly>");
				return;
			}
			input = input.Substring("/input:".Length);
			var output = args[1];
			if (!output.StartsWith("/output:"))
			{
				logger.Error("Second argument must be /output:<output_assembly>");
				return;
			}
			output = output.Substring("/output:".Length);
			// parse resources
			var resources = new List<ResourceInfo>();
			const int resourceOffset = 2;
			for (int i = resourceOffset; i < args.Length; i++)
			{
				var data = args[i];
				if (!data.Contains(">"))
				{
					logger.Error("Resource file {0} did not contain required deliminator '>'.", i - resourceOffset);
					Environment.Exit(-1);
				}
				var idx = data.IndexOf('>');
				var inputResource = data.Substring(0, idx);
				var embeddedName = data.Substring(idx + 1);
				if (!File.Exists(inputResource))
				{
					logger.Error("Input file: '{0}' not found.", inputResource);
					Environment.Exit(-1);
				}
				resources.Add(new ResourceInfo(inputResource, embeddedName));
			}
			using (IModifyAssemblies modifier = new CecilBasedAssemblyModifier(logger, input, output))
			{
				if (!modifier.EmbedResources(resources.ToArray()))
				{
					logger.Error("Failed to embed resources!");
					return;
				}
				if (!modifier.InjectModuleInitializedCode(CecilHelpers.InjectEmbeddedResourceLoader))
				{
					logger.Error("Failed to inject code!");
					return;
				}
			}
			Console.WriteLine("Successfully added resources and code!");
		}

		private static void PrintUsageAndExit()
		{
			Console.WriteLine("Tool that allows embedding of resources into existing assemblies.");
			Console.WriteLine("Usage:");
			Console.WriteLine("exe \"/input:input_assembly\" [\"/output:output_assembly\"] [\"resource_file_to_embed>path_in_assembly\"]*");
			Console.WriteLine("\tinput_assembly:\t\tFull or relative path to the assembly where the resources should be embedded");
			Console.WriteLine("\toutput_assembly:\tOptional. If not provided, \"input_assembly\" will be overriden.");
			Console.WriteLine("\t\"resource_file_to_embed>path_in_assembly\" list of resources to embed.");
			Console.WriteLine("\t\t-resource_file_to_embed: Full or relative path to embed.");
			Console.WriteLine("\t\t-path_in_assembly:\t The path the file should have in the assembly.");
			Environment.Exit(0);
		}

		#endregion Methods
	}
}