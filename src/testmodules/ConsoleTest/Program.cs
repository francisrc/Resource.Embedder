using System.IO;
using System.Reflection;

namespace ConsoleTest
{
	class Program
	{
		#region Methods

		public static byte[] LoadResource(string path)
		{
			var asm = Assembly.GetExecutingAssembly();
			byte[] data;

			using (var stream = asm.GetManifestResourceStream(path))
			{
				if (stream == null)
					return null;

				data = new byte[stream.Length];
				stream.Read(data, 0, data.Length);
			}
			return data;
		}

		static void Main(string[] args)
		{
			if (args.Length == 2)
			{
				var resourceName = args[0];
				var outputTarget = args[1];

				File.WriteAllBytes(outputTarget, LoadResource(resourceName));
			}
		}

		#endregion Methods
	}
}