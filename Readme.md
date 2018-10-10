[Gitlab](https://gitlab.com/MarcStan) is just a read-only mirror, [github](https://github.com/MarcStan) has the most up-to-date content.

## Embeds satellite assemblies into the main assemblies.

If you use [Fody.Costura](https://github.com/Fody/Costura) this tool probably comes in handy as well.
Fody.Costura cannot embed localization resources (*.resources.dll) as Costura runs before MS Build can generate them.

This tool will embed the localization assemblies. Works just as easy as Costura: Add nuget package to project and done!

Many thanks to Simon for the excellent [Fody](https://github.com/Fody/Fody) and [Fody.Costura](https://github.com/Fody/Costura) which this tool is heavily inspired from.

* compatible with .Net 4 and above

## Available on NuGet  [![NuGet Status](https://img.shields.io/nuget/v/Resource.Embedder.svg?style=flat)](https://www.nuget.org/packages/Resource.Embedder/)

https://nuget.org/packages/Resource.Embedder/

    PM> Install-Package Resource.Embedder
   
By adding the NuGet package to an assembly all it's satellite assemblies will automatically be embedded as resources and loaded from there.
No more need for deploying the satellite assembly folders.


# Inject your own assemblies/files

Alternative solution if you want to embed custom files or control exactly which resources are embedded how and where.

Example usecases for this:

* Allow drag & drop style embedding (when user drags a dll or other files onto the exe)
* Include default plugins (that are always loaded) inside your dll
* Embed any file dyamically during runtime

## Available on NuGet  [![NuGet Status](https://img.shields.io/nuget/v/Resource.Embedder.Core.svg?style=flat)](https://www.nuget.org/packages/Resource.Embedder.Core/)

https://nuget.org/packages/Resource.Embedder.Core/

    PM> Install-Package Resource.Embedder.Core
   
By adding the NuGet package to an assembly it is possible to manually inject resources and code into other assemblies.

See [this code for injecting resources](https://github.com/MarcStan/Resource.Embedder/blob/master/src/ResourceEmbedder.Core.Tests/EmbedFilesTests.cs#L132) and [this code for injecting code](https://github.com/MarcStan/Resource.Embedder/blob/master/src/ResourceEmbedder.Core.Tests/InjectCodeTests.cs#L28).
___

### How it works

The NuGet package works similar to [Costura](https://github.com/Fody/Costura) and injects a .targets file into the project it is added to, thus allowing for two things during build:

* Embedding the satellite assemblies into the assembly as resources [as per Jeffrey Richters example](https://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx).
* Uses Cecil to add/edit the [module initializer](http://einaregilsson.com/module-initializers-in-csharp/) which will call the hooking code to load the satellite assemblies from resources [(The Injected code)](https://github.com/MarcStan/Resource.Embedder/blob/master/src/ResourceEmbedder.Core/GeneratedCode/InjectedResourceLoader.cs)

## Why?

By embedding it is possible to create "zero dependency" executables that can simply be deployed and "just run"**™**.

Costura does exactly the same, except that it [cannot embed satellite assemblies](https://github.com/Fody/Costura/issues/61) due to the way it's integrated into the build process (it runs before they are generated).

This tool on the contrary runs after build and is thus able to embed. It is also (obviously) compatible with Costura, meaning by adding both to your project all your references and satellite assemblies will be embedded).

### Details

All culture files for the current assembly will be added as resources to the assembly.

[This code](https://github.com/MarcStan/Resource.Embedder/blob/master/src/ResourceEmbedder.Core/GeneratedCode/InjectedResourceLoader.cs) will then be injected into the assembly and called via the module initializer.

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
* Using tools like [JustDecompile](https://www.telerik.com/products/decompiler.aspx) it is possible to see that WpfTest.exe now contains resources "WpfTest.de.resources.dll"

### Configuration

Currently nothing can be configured and it "just works" out of the box.

### Output directory

Note that the satellite assemblies are always copied to the output directory by Visual Studio.

After the build finishes the Resource.Embedder will delete all resource files it has embedded, leaving only the resource files that have not been embedded (if any).

If there are no resource files left for a specific language (empty localization directory) the directory is deleted as well.

# Changelog

See [Changelog.md](Changelog.md)