using PluginCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace PluginLoaderConsole
{
	class Program
	{
		#region Methods

		private static void LoadAssemblies(string path)
		{
			// LocalizedPlugin assembly is never referenced, so forceload it manually
			Assembly.LoadFrom(Path.Combine(path, "LocalizedPlugin.dll"));
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
			if (args.Length == 1 && args[0].StartsWith("/fulltest:"))
			{
				// assert that there is no plugin as the assembly is not referenced
				if (LoadPlugins().Count() != 0)
				{
					Environment.Exit(-1);
				}

				var path = args[0].Substring("/fulltest:".Length);
				// force the load
				LoadAssemblies(path);
				var plugins = LoadPlugins().ToList();
				if (plugins.Count == 0)
				{
					// happens when the dll is not compiled to output directory
					Environment.Exit(-2);
				}
				var p = plugins[0];
				// assert localization works
				p.CurrentLocale = new CultureInfo("en");
				if (p.LocalizedHeader != "Hello world!" ||
					p.LocalizedDescription != "This is a localized description of the plugin.")
				{
					Environment.Exit(-3);
				}
				p.CurrentLocale = new CultureInfo("de");
				if (p.LocalizedHeader != "Hallo Welt!" ||
					p.LocalizedDescription != "Das ist eine übersetze Beschreibung des Plugins.")
				{
					Environment.Exit(-3);
				}

				// ensure that the HeaderLocalizedByThread is actually not affected by the property
				p.CurrentLocale = new CultureInfo("de");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
				if (p.LocalizedHeader != "Hallo Welt!" ||
					p.HeaderLocalizedByThread != "Hello world!" ||
					p.LocalizedDescription != "Das ist eine übersetze Beschreibung des Plugins.")
				{
					Environment.Exit(-4);
				}
				// but instead only by the thread's culture
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");
				if (p.HeaderLocalizedByThread != "Hallo Welt!")
				{
					Environment.Exit(-5);
				}
				// also test fallback route de-DE doesn't exist, so it should load "de" and not "en"
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
				if (p.HeaderLocalizedByThread != "Hallo Welt!")
				{
					Environment.Exit(-5);
				}

				Environment.Exit(0);
			}
			else
			{
				LoadAssemblies(".");
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

		private static void SetLocale(string culture)
		{
			var ci = new CultureInfo(culture);
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;
			CultureInfo.DefaultThreadCurrentCulture = ci;
			CultureInfo.DefaultThreadCurrentUICulture = ci;
		}

		#endregion Methods
	}
}