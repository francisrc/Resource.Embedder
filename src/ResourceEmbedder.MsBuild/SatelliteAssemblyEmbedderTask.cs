using Microsoft.Build.Framework;
using Mono.Cecil;
using ResourceEmbedder.Core;
using ResourceEmbedder.Core.Cecil;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourceEmbedder.MsBuild
{
    /// <summary>
    /// Task to embed satellite assemblies into an existing .Net assembly.
    /// Will also add code to the module initializer that will hook into AssemblyResolve event to load from embedded resources.
    /// </summary>
    public class SatelliteAssemblyEmbedderTask : MsBuildTask
    {
        [Output]
        public string EmbeddedCultures { get; set; }

        public override bool Execute()
        {
            var logger = new MSBuildBasedLogger(BuildEngine, "ResourceEmbedder");
            if (!AssertSetup(logger))
            {
                return false;
            }

            var watch = new Stopwatch();
            watch.Start();
            // run in object dir (=AssemblyPath) as we will run just after satellite assembly generated and ms build will then copy the output to target dir
            string inputAssembly = Path.Combine(ProjectDirectory, AssemblyPath);
            var workingDir = new FileInfo(inputAssembly).DirectoryName;
            if (IsOlderThanNet46(inputAssembly))
            {
                // resource embedder doesn't support < .Net 4.0 due to .Net not invoking resource assembly event prior to .Net 4: https://msdn.microsoft.com/en-us/library/system.appdomain.assemblyresolve.aspx
                // .Net 4.6 is also the new minimum target to ensure cross compile with .Net Standard works
                logger.Error("Versions prior to .Net 4.6 are no longer supported. Verison 1.x supports all version from .Net 4 and above. Please either upgrade to .Net 4.6, downgrade this package to 1.0 or remove the Resource.Embedder NuGet package from this project. " +
                             "See https://github.com/MarcStan/Resource.Embedder/issues/3 and https://msdn.microsoft.com/en-us/library/system.appdomain.assemblyresolve.aspx for details.");
                return false;
            }

            var assembliesToEmbed = new List<ResourceInfo>();
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var inputAssemblyName = Path.GetFileNameWithoutExtension(inputAssembly);

            var usedCultures = new List<string>();
            foreach (var ci in cultures)
            {
                // check if culture satellite assembly exists, if so embed
                var ciPath = Path.Combine(workingDir, ci.Name, string.Format("{0}.resources.dll", inputAssemblyName));
                if (File.Exists(ciPath))
                {
                    //logger.Debug("Embedding culture: {0}", ci);
                    usedCultures.Add(ci.Name);
                    assembliesToEmbed.Add(new ResourceInfo(ciPath, string.Format("{0}.{1}.resources.dll", inputAssemblyName, ci)));
                }
            }
            if (assembliesToEmbed.Count == 0)
            {
                logger.Info("Nothing to embed! Skipping {0}", inputAssembly);
                return true;
            }

            // add target directory where the assembly is compiled to to search path for reference assemblies
            var searchDirs = new List<string> { new FileInfo(TargetPath).DirectoryName };
            // fix for https://github.com/MarcStan/Resource.Embedder/issues/5
            // when references are marked as CopyLocal: False they will not end up at TargetPath when we run this code (instead they may be copied later)
            // so we need to tell Cecil about all the directories where they could be
            var referenceFiles = References ?? "";
            var referenceDirs = referenceFiles.Contains(";") ? referenceFiles.Split(';') : new[] { referenceFiles };

            // we need the directory path, but the references are all files, so convert and take distinct set
            searchDirs.AddRange(referenceDirs.Select(f => new FileInfo(f).DirectoryName).Distinct());
            logger.Info("Looking for references in: {0}", string.Join(", ", searchDirs));

            StrongNameKeyPair signingKey = null;
            var debugSymbolType = DebugSymbolHelper.FromString(DebugType);
            var symbolReader = CecilBasedAssemblyModifier.GetSymbolReader(inputAssembly, debugSymbolType);
            var rp = CecilBasedAssemblyModifier.GetReaderParameters(inputAssembly, searchDirs, symbolReader);
            if (!SignAssembly)
            {
                if (DebugSymbols && !File.Exists(Path.ChangeExtension(inputAssembly, ".pdb")))
                {
                    // can't call ReadModule with DebugSymbols=true when .pdb is missing; since we most likely won't end up producing working output anyway
                    // just ignore the sign assembly check
                    logger.Warning("DebugSymbols are requested, but .pdb file is missing!");
                }
                else
                {
                    using (var md = ModuleDefinition.ReadModule(inputAssembly, rp))
                    {
                        var name = nameof(AssemblyKeyFileAttribute);
                        var keyFileAttr = md.Assembly.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == name);
                        if (keyFileAttr != null)
                        {
                            logger.Info("Found AssemblyKeyFileAttribute even though MSBuild said SignAssembly=false, assuming assembly has to be signed anyway.");
                            SignAssembly = true;
                        }
                    }
                }
            }
            if (SignAssembly)
            {
                var keyFilePath = GetSigningKeyPath(inputAssembly, rp, logger);
                if (!File.Exists(keyFilePath))
                {
                    logger.Info("Could not find signing key file at path '{0}'.", keyFilePath);
                    return false;
                }
                signingKey = new StrongNameKeyPair(File.OpenRead(keyFilePath));
            }

            using (IModifyAssemblies modifer = new CecilBasedAssemblyModifier(logger, inputAssembly, inputAssembly, searchDirs.ToArray(), debugSymbolType, signingKey))
            {
                if (!modifer.EmbedResources(assembliesToEmbed.ToArray()))
                {
                    logger.Error("Failed to embed resources into assembly: " + inputAssembly);
                    return false;
                }
                if (!modifer.InjectModuleInitializedCode(CecilHelpers.InjectEmbeddedResourceLoader))
                {
                    logger.Error("Failed to inject required code into assembly: " + inputAssembly);
                    return false;
                }
            }
            watch.Stop();
            EmbeddedCultures = string.Join(";", usedCultures);
            logger.Info("Finished embedding cultures: {0} into {1} in {2}ms", string.Join(", ", usedCultures), Path.GetFileName(inputAssembly), watch.ElapsedMilliseconds);
            return true;
        }

        /// <summary>
        /// Attempts to find the signing key for the specific assembly.
        /// </summary>
        /// <param name="inputAssemblyPath"></param>
        /// <param name="rp"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private string GetSigningKeyPath(string inputAssemblyPath, ReaderParameters rp, Core.ILogger logger)
        {
            if (KeyFilePath != null)
            {
                var path = Path.GetFullPath(KeyFilePath);
                // only use, if the file exists
                if (File.Exists(path))
                {
                    logger.Info("Using signing key provided by msbuild: '{0}'", path);
                    return path;
                }
            }

            // fallback as per: https://github.com/Fody/Fody/blob/master/FodyIsolated/StrongNameKeyFinder.cs
            using (var md = ModuleDefinition.ReadModule(inputAssemblyPath, rp))
            {
                var name = nameof(AssemblyKeyFileAttribute);
                var keyFileAttr = md.Assembly.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == name);
                if (keyFileAttr != null)
                {
                    var suffix = (string)keyFileAttr.ConstructorArguments.First().Value;
                    var path = Path.Combine(IntermediateDirectory, suffix);
                    logger.Info("Using signing key path from attribute: {0}", path);
                    return path;
                }
            }
            logger.Warning("No signing key found");
            return null;
        }

        /// <summary>
        /// Returns whether the specific file is an assembly that was compiled with an older version than .Net 4
        /// </summary>
        /// <param name="inputAssembly"></param>
        /// <returns></returns>
        private bool IsOlderThanNet46(string inputAssembly)
        {
            // easiest method would be to load the assembly and read out Assembly.ImageRuntimeVersion
            // but then we would lock the assembly file
            // only workaround would be to load into a different AppDomain but I'm too stupid to get it to work, so I'll use corflags.exe

            var corFlagsReader = WindowsSdkHelper.FindCorFlagsExe();
            if (corFlagsReader == null || !File.Exists(corFlagsReader))
            {
                Log.LogWarning("Could not determine version of assembly. If you are compiling an assembly targeting an older version than .Net 4 then resources will not work (consider removing Resource.Embedder from that project). If you are targeting .Net 4 or above, everything should be fine. See https://github.com/MarcStan/Resource.Embedder/issues/3 for details.");
                return false; // without corflags to check version, just process all silently, although corflags is distributed with every .Net version so it should always exist unless user deleted it
            }

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(corFlagsReader, inputAssembly)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            bool old = false;
            p.Start();
            using (var reader = p.StandardOutput)
            {
                var r = reader.ReadToEnd();
                var d = r;
                var lines = d.Contains("\n") ? d.Replace("\r", "").Split('\n') : new[] { d };
                var m = lines.FirstOrDefault(l => l.Trim().StartsWith("Version") && l.Contains(":"));
                // output is in format:
                // Version        : v4.0...
                // Version        : v2.0...

                // TODO: detect .Net 4.6 by build number
                // check if old format, all else is fine
                if (m != null && m.Split(':')[1].Trim().StartsWith("v2"))
                {
                    old = true;
                }
            }
            if (!p.WaitForExit(5000))
            {
                try
                {
                    p.Kill();
                }
                catch
                {
                }
                return true;
            }
            p.Dispose();
            return old;
        }
    }
}
