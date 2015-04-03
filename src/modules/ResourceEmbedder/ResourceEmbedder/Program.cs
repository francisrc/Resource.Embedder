using ResourceEmbedder.Core;
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
			if (args.Length < 4)
			{
				PrintUsageAndExit();
			}
			var logger = new ConsoleLogger();
			var dir = new FileInfo(args[0]).DirectoryName;
			Environment.CurrentDirectory = dir;
			var input = args[1];
			var output = args[2];
			// parse resources
			var resources = new List<ResourceInfo>();
			const int resourceOffset = 3;
			for (int i = resourceOffset; i < args.Length; i++)
			{
				var data = args[i];
				if (!data.Contains(">"))
				{
					logger.LogError("Resource file {0} did not contain required deliminator '>'.", i - resourceOffset);
					Environment.Exit(-1);
				}
				var idx = data.IndexOf('>');
				var inputResource = data.Substring(0, idx);
				var embeddedName = data.Substring(idx + 1);
				if (!File.Exists(inputResource))
				{
					logger.LogError("Input file: '{0}' not found.", inputResource);
					Environment.Exit(-1);
				}
				resources.Add(new ResourceInfo(inputResource, embeddedName));
			}
			IEmbedFiles embedder = new CecilBasedEmbedder(logger);
			var assembliesToEmbedd = new ResourceInfo[0];
			if (!embedder.EmbedResources(input, output, assembliesToEmbedd))
			{
				logger.LogError("Failed to embed resources!");
			}
		}

		private static void PrintUsageAndExit()
		{
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