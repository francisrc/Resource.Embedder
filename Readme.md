## Embeds satellite assemblies into the main assemblies.

If you use [Fody.Costura](https://github.com/Fody/Costura) this tool probably comes in handy as well.
Fody.Costura cannot embed localization resources (*.resources.dll) as Costura runs before MS Build can generate them.

This tool will embed the localization assemblies. Works just as easy as Costura: Add nuget package to project and done!

Many thanks to Simon for the excellent [Fody](https://github.com/Fody/Fody) and [Fody.Costura](https://github.com/Fody/Costura) which this tool is heavily inspired from.

## Available on NuGet  [![NuGet Status](http://img.shields.io/nuget/v/Resource.Embedder.svg?style=flat)](https://www.nuget.org/packages/Resource.Embedder/)

https://nuget.org/packages/Resource.Embedder/

    PM> Install-Package Resource.Embedder
   
By adding the NuGet package to an assembly all it's satellite assemblies will automatically be embedded as resources and loaded from there.
No more need for deploying the satellite assembly folders.

### How it works

The NuGet package works similar to [Costura](https://github.com/Fody/Costura) and injects a .targets file into the project it is added to, thus allowing for two things during build:

* Embedding the satellite assemblies into the assembly as resources [as per Jeffrey Richters example](http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx).
* Uses Cecil to add/edit the [module initializer](http://einaregilsson.com/module-initializers-in-csharp/) which will call the hooking code to load the satellite assemblies from resources [(The Injected code)](https://github.com/MarcStan/Resource.Embedder/blob/master/src/modules/ResourceEmbedder.Core/ResourceEmbedder.Core/InjectedResourceLoader.cs)

## Why?

By embedding it is possible to create "zero dependency" executables that can simply be deployed and "just run"**™**.

Costura does exactly the same, except that it [cannot embed satellite assemblies](https://github.com/Fody/Costura/issues/61) due to the way it's integrated into the build process (it runs before they are generated).

This tool on the contrary runs after build and is thus able to embed. It is also (obviously) compatible with Costura, meaning by adding both to your project all your references and satellite assemblies will be embedded).

### Details

All culture files for the current assembly will be added as resources to the assembly.

[This code](https://github.com/MarcStan/Resource.Embedder/blob/master/src/modules/ResourceEmbedder.Core/ResourceEmbedder.Core/InjectedResourceLoader.cs) will then be injected into the assembly and called via the module initializer.

The injected code will then hook into the AppDomain.CurrentDomain.AssemblyResolve event as soon as the assembly is loaded and load the resources during runtime whenever the language change requests a assembly load.


**Example:**

If the application is called Wpf.exe and has "de" (German), and "fr" (French) satellite assemblies, these will be added as resources: Wpf.de.resources.dll and Wpf.fr.resources.dll to the Wpf.exe and then autom. resolved during runtime via the hooking code.

### Try it yourself

* Download the repository and open the solution (src\CompleteSolutions\Resource.Embedder.sln).
* Add the nuget package "Resource.Embedder" to the project "WPFTest" and compile it.
* Output will be "WpfTest.exe" in the bin\Debug or bin\Release folder in the root.
* Copy it somewhere or delete the localization folders (Visual Studio will always copy them on build)
* Run the app and type "de" or "fr" into the textbox.
* Observe that the "Hello world" text is properly localized when pressing the button to change locale even without resource assembly directories being present

### Configuration

Currently nothing can be configured and it "just works" out of the box.

### Output directory

Note that the satellite assemblies are always copied to the output directory by Visual Studio.

You can either manually delete them or simply not reference them in your further build.


# Roadmap

* Support for signed assemblies
* Delete satellite assemblies from output once merged