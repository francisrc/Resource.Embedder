## Embeds satellite assemblies into the main assemblies.

If you use [Fody.Costura](https://github.com/Fody/Costura) this tool probably comes in handy as well.
Fody.Costura cannot embed localization resources (*.resources.dll) as Costura runs before MS Build can generate them.

This tool will embed the localization assemblies. Works just as easy as Costura: Add nuget package to project and done!

Many thanks to Simon for the excellent [Fody](https://github.com/Fody/Fody) and [Fody.Costura](https://github.com/Fody/Costura) which this tool is heavily inspired from.

* compatible with .Net 4 and above
* currently no support for signed assemblies

## Available on NuGet  [![NuGet Status](http://img.shields.io/nuget/v/Resource.Embedder.svg?style=flat)](https://www.nuget.org/packages/Resource.Embedder/)

https://nuget.org/packages/Resource.Embedder/

    PM> Install-Package Resource.Embedder
   
By adding the NuGet package to an assembly all it's satellite assemblies will automatically be embedded as resources and loaded from there.
No more need for deploying the satellite assembly folders.


___
## Available on NuGet  [![NuGet Status](http://img.shields.io/nuget/v/Resource.Embedder.Core.svg?style=flat)](https://www.nuget.org/packages/Resource.Embedder.Core/)

https://nuget.org/packages/Resource.Embedder.Core/

    PM> Install-Package Resource.Embedder.Core
   
By adding the NuGet package to an assembly it is possible to manually inject resources and code into other assemblies.

See [this code for injecting resources](https://github.com/MarcStan/Resource.Embedder/blob/master/src/modules/ResourceEmbedder.Core/ResourceEmbedder.Core.Tests/EmbedFilesTests.cs#L124) and [this code for injecting code](https://github.com/MarcStan/Resource.Embedder/blob/master/src/modules/ResourceEmbedder.Core/ResourceEmbedder.Core.Tests/InjectCodeTests.cs#L26).
___

### How it works

The NuGet package works similar to [Costura](https://github.com/Fody/Costura) and injects a .targets file into the project it is added to, thus allowing for two things during build:

* Embedding the satellite assemblies into the assembly as resources [as per Jeffrey Richters example](http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx).
* Uses Cecil to add/edit the [module initializer](http://einaregilsson.com/module-initializers-in-csharp/) which will call the hooking code to load the satellite assemblies from resources [(The Injected code)](https://github.com/MarcStan/Resource.Embedder/blob/master/src/modules/ResourceEmbedder.Core/ResourceEmbedder.Core/GeneratedCode/InjectedResourceLoader.cs)

## Why?

By embedding it is possible to create "zero dependency" executables that can simply be deployed and "just run"**™**.

Costura does exactly the same, except that it [cannot embed satellite assemblies](https://github.com/Fody/Costura/issues/61) due to the way it's integrated into the build process (it runs before they are generated).

This tool on the contrary runs after build and is thus able to embed. It is also (obviously) compatible with Costura, meaning by adding both to your project all your references and satellite assemblies will be embedded).

### Details

All culture files for the current assembly will be added as resources to the assembly.

[This code](https://github.com/MarcStan/Resource.Embedder/blob/master/src/modules/ResourceEmbedder.Core/ResourceEmbedder.Core/GeneratedCode/InjectedResourceLoader.cs) will then be injected into the assembly and called via the module initializer.

The injected code will then hook into the AppDomain.CurrentDomain.AssemblyResolve event as soon as the assembly is loaded and load the resources during runtime whenever the language change requests an assembly load.


**Example:**

If the application is called Wpf.exe and has "de" (German), and "fr" (French) satellite assemblies, these will be added as resources: Wpf.de.resources.dll and Wpf.fr.resources.dll to the Wpf.exe and then autom. resolved during runtime via the hooking code.

### Try it yourself

* Download the repository and open the solution (Resource.Embedder.sln).
* Add the nuget package "Resource.Embedder" to the project "WpfTest" and compile it.
* Output will be "WpfTest.exe" in the bin\Debug or bin\Release folder in the root.
* Copy it somewhere without the localization folders
* Run the app and type "de" or "fr" into the textbox.
* Observe that the "Hello world" text is properly localized when pressing the button to change locale even without resource assembly directories being present
* Using tools like [JustDecompile](http://www.telerik.com/products/decompiler.aspx) it is possible to see that WpfTest.exe now contains resources "WpfTest.de.resources.dll"

### Configuration

Currently nothing can be configured and it "just works" out of the box.

### Output directory

Note that the satellite assemblies are always copied to the output directory by Visual Studio.

After the build finishes the Resource.Embedder will delete all resource files it has embedded, leaving only the resource files that have not been embedded (if any).

If there are no resource files left for a specific language (empty localization directory) the directory is deleted as well.


# Roadmap

* Support for signed assemblies

# Changelog

**v1.1.0**

* Added second Nuget package that directly references ResourceEmbedder.Core and allows you to inject code/resources into other assemblies by yourself.

**v1.0.11**

* added search directory for embedding, necessary for some setups (esp. plugins compiling to other directories than reference assemblies)

**v1.0.9**

* Fixed bug that would cause crash for applications running on .Net 4 on a maschine with only .Net 4 (not updated to .Net 4.5) due to usage of property only available in .Net 4.5 and above

**v1.0.8**

* Projects targeting versions of .Net older than .Net 4.0 will now throw build error instead of silently failing during runtime. [To my knowledge this is also not fixable](https://github.com/MarcStan/Resource.Embedder/issues/3)

**v1.0.7**

* Fixed crash when added to dll that has no localization at all

**v1.0.6**

* Improved build log
* prevented Visul Studio from locking output file

**v1.0.5**

* after successful embedding of resources the resource dll files are deleted from output (as they would be duplicates and are no longer required)

**v1.0.4**

* Faulty release, see v1.0.5 for fix

**v1.0.3**

* Increased output log verbosity

**v1.0.2**

* Fixed a bug where assemblies with similar names would wrongfully swallow resource detection (used .StartsWith for assembly name comparison, now complete name is compared)

**v1.0.1**

* Improved performance (embedding of resources and injection of loader code now happens in one step instead of 2 seperate ones)
* Added localization fallback route (identical behaviour to .Net fallback). Fallback happens for embedded resources (e.g. de-DE -> de -> en (default language)), if no embedded are found, same fallback route is again applied for resource files on disk

**v1.0.0** 

* Initial release, just add to project and the work is done autom. during build - no configuration needed!