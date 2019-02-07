**v1.2.7**

Run cleanup before "AfterBuild" target instead of after it.

**v1.2.6**

* Added user input validation on type resolution (e.g. Type.GetType) to prevent crashes on malformed input

**v1.2.5**

* Added support for signed assemblies via:
  * csproj AssemblyOriginatorKeyFile and
  * AssemblyKeyFileAttribute assembly attribute

**v1.2.4**

* Added error message and aborting when "portable" debugging information is used (as opposed to just crashing)

**v1.2.3**

* Removed unnecessary files from deploy
* Cleaned up solution structure
* Added support for "none" and "embedded" debugging information

**v1.2.2**

* Updated cecil dependency to latest
* Internal changes

**v1.2.1**

* Moved to Gitlab

**v1.2.0**

* Fixed a crash of Resource.Embedder when the project using the embedder had references that where not set to CopyLocal

The bug only occured if the project either used a const field with a TypeRef to an enum defined in one of the references (which weren't copied locally) or when a custom attribute was instantiated with a TypeRef to an enum

Both these cases are described [here](https://github.com/jbevain/cecil/issues/236) and forced Cecil to actually load the reference instead of just assuming that it exists.

The Resource.Embedder task did not properly look in all directories for the reference assemblies and thus crashed when it finally wasn't found

**v1.1.1**

* Reduced log output messages

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