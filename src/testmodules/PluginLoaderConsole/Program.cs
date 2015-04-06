using PluginCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PluginLoaderConsole
{
	class Program
	{
		#region Methods

		private static void LoadAssemblies()
		{
			// LocalizedPlugin assembly is never referenced, so forceload it manually
			Assembly.Load("LocalizedPlugin");
		}

		private static IEnumerable<ILocalizedPlugin> LoadPlugins()
		{
			var asm = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in asm)
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.GetInterfaces().Any(t => t == typeof(ILocalizedPlugin)))
					{
						yield return (ILocalizedPlugin)Activator.CreateInstance(type);
					}
				}
			}
		}

		static void Main(string[] args)
		{
			if (args.Length == 1 && args[0] == "/fulltest")
			{
				// assert that there is no plugin as the assembly is not referenced
				if (LoadPlugins().Count() != 0)
				{
					Environment.Exit(-1);
				}
				// force the load
				LoadAssemblies();
				var plugins = LoadPlugins().ToList();
				if (plugins.Count == 0)
				{
					// happens when the dll is not compiled to output directory
					Environment.Exit(-2);
				}
				var p = plugins[0];
				// assert localization works
				SetLocale("en");
				if (p.LocalizedHeader != "Hello world!" ||
					p.LocalizedDescription != "This is a localized description of the plugin.")
				{
					Environment.Exit(-3);
				}
				SetLocale("de");
				if (p.LocalizedHeader != "Hallo Welt!" ||
					p.LocalizedDescription != "Das ist eine übersetze Beschreibung des Plugins.")
				{
					Environment.Exit(-3);
				}
				Environment.Exit(0);
			}
			else
			{
				LoadAssemblies();
				var plugins = LoadPlugins().ToList();
				if (plugins.Count == 0)
				{
					Console.WriteLine("No plugins found!");
					return;
				}
				SetLocale("en");
				foreach (var p in plugins)
				{
					Console.WriteLine(p.ToString());
				}
				SetLocale("de");
				foreach (var p in plugins)
				{
					Console.WriteLine(p.ToString());
				}
			}
		}

		private static void SetLocale(string ci)
		{
			CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(ci);
		}

		#endregion Methods
	}
}