[Gitlab](https://gitlab.com/MarcStan) is just a read-only mirror, [github](https://github.com/MarcStan) has the most up-to-date content.

# Automatically embeds satellite assemblies into the main assembly

Compatible with .Net Standard 2.0, .Net Core 2.0+ and .Net 4.6 and above

If you use [Costura](https://github.com/Fody/Costura) this tool probably comes in handy as well.

**Costura cannot embed localization resources** (*.resources.dll) as Costura runs before MS Build can generate them.

This tool will embed the localization assemblies. Works just as easy as Costura: Add nuget package to project and done!

Many thanks to Simon for the excellent [Fody](https://github.com/Fody/Fody) and [Costura](https://github.com/Fody/Costura) which this tool is heavily inspired from.

## Why?

By embedding translations it is possible to create "zero dependency" executables that can simply be deployed and "just run".

In order to embed any reference dll's, use Costura ([which cannot embed satellite assemblies](https://github.com/Fody/Costura/issues/61) due to the way it's integrated into the build process (it runs before they are generated)).

## Resource.Embedder  [![NuGet Status](https://img.shields.io/nuget/v/Resource.Embedder.svg?style=flat)](https://www.nuget.org/packages/Resource.Embedder/)

By adding the NuGet package to an assembly all it's satellite assemblies will automatically be embedded as resources and loaded from there.
No more need for deploying the satellite assembly folders.

## Resource.Embedder.Core  [![NuGet Status](https://img.shields.io/nuget/v/Resource.Embedder.Core.svg?style=flat)](https://www.nuget.org/packages/Resource.Embedder.Core/)

(Use the Core package if you want to manually inject resources)

## Known issues

* Compatibility issue with Fody/Costura

There is an incompatibility with Fody 4.x, I suggest you downgrade to Costura 3.3.2 in the meantime (which uses Fody \< 4).

Fody 4 embeds its class multiple times into the same assembly if used in conjunction with resource embedder. This will cause a BadImageEsxception on launch ([#13](https://github.com/MarcStan/Resource.Embedder/issues/13)).

* SDK style project + PostBuild

If you use this combination **and** you happen to run something like `xcopy * *` to copy all your files elsewhere, you will find that the culture folders are included in the postbuild. This is due to the cleanup running after the postbuild event.

I recommend you use "dotnet publish" instead (with -o you can specific where to copy the files to) as the cleanup runs before publish

* .Net Core deps file

If you build or publish a .Net Core app, it will have a *.deps.json file that is required to run.

Inside the file you will still find sections that seem to reference the satellite assemblies even though the files don't exist on disk. To my knowledge there is no downside to still having this entry.

``` json
"resources": {
  "de/DemoWebAppDotNetCore.resources.dll": {
  "locale": "fr"
}
```

Nevertheless, even without a satellite file on disk (as it's embedded into the dll) the .Net Core app will run just fine and is automatically localized.

# Inject your own assemblies/files (using Resource.Embedder.Core)

Alternative solution if you want to embed custom files or control exactly which resources are embedded how and where.

Example usecases for this:

* Allow drag & drop style embedding (when user drags a dll or other files onto the exe)
* Include default plugins (that are always loaded) inside your dll
* Embed any file dyamically during runtime

By adding the NuGet package to an assembly it is possible to manually inject resources and code into other assemblies.

See [this code for injecting resources](https://github.com/MarcStan/Resource.Embedder/blob/master/src/ResourceEmbedder.Core.Tests/EmbedFilesTests.cs#L162) and [this code for injecting code](https://github.com/MarcStan/Resource.Embedder/blob/master/src/ResourceEmbedder.Core.Tests/InjectCodeTests.cs#L40).
___

# Internals

The NuGet package uses MSBuild events to hook into the build pipeline and:

* Embedding the satellite assemblies into the assembly as resources [as per Jeffrey Richters example](https://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx).
* Uses Cecil to add/edit the [module initializer](http://einaregilsson.com/module-initializers-in-csharp/) which will call the hooking code to load the satellite assemblies from resources [(The Injected code)](https://github.com/MarcStan/Resource.Embedder/blob/master/src/ResourceEmbedder.Core/GeneratedCode/InjectedResourceLoader.cs)

## Verify

Using tools like [JustDecompile](https://www.telerik.com/products/decompiler.aspx) it is possible to see that any project using this package contains resources "\<AssemblyName>.\<culture>.resources.dll"

### Configuration

Currently nothing can be configured and it "just works" out of the box.

### Output directory

Note that the satellite assemblies are always copied to the output directory by Visual Studio.

After the build finishes the Resource.Embedder will delete all resource files it has embedded, leaving only the resource files that have not been embedded (if any).

If there are no resource files left for a specific language (empty localization directory) the directory is deleted as well.

# Changelog

See [Changelog.md](Changelog.md)