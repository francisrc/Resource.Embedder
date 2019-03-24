using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace ResourceEmbedder.MsBuild
{
    /// <summary>
    /// Since <see cref="SatelliteAssemblyEmbedderTask"/> only embedds the files,
    /// but doesn't prevent them from getting copied to the output we will have to do so manually.
    /// </summary>
    public class SatelliteAssemblyCleanupTask : MsBuildTask
    {
        private readonly string _loggerName;
        private readonly List<string> _emptyDirectories = new List<string>();
        private readonly Lazy<Core.ILogger> _lazyLogger;

        public SatelliteAssemblyCleanupTask()
            : this("ResourceEmbedder.Cleanup")
        {
        }

        public SatelliteAssemblyCleanupTask(string loggerName)
        {
            _loggerName = loggerName;
            _lazyLogger = new Lazy<Core.ILogger>(() => new MSBuildBasedLogger(BuildEngine, _loggerName));
        }

        protected Core.ILogger Logger => _lazyLogger.Value;

        public string EmbeddedCultures { get; set; }

        public override bool Execute()
        {
            // this sleep semi-fixes a race condition:
            // old: "run after AfterBuild" -> now: "run before AfterBuild"
            // since this is closer to creation of resource directories the cleanup seems to be to fast (it deletes all directories and
            // sometimes one of the directories is created again but stays empty)
            // to fix this small annoyance just wait a bit - this seems to fix it most the time
            Thread.Sleep(50);

            if (!AssertSetup(Logger))
            {
                return false;
            }

            var workingDir = new FileInfo(TargetPath).DirectoryName;

            return RunCleanup(workingDir);
        }

        protected virtual bool RunCleanup(string workingDir)
        {
            string outputAssembly = Path.Combine(ProjectDirectory, AssemblyPath);
            // detect which cultures have been embedded
            var embeddedCultures = GetEmbeddedCultures();
            // assembly name may be relative path + name of assembly, e.g. ..\output.exe
            // we need only the name for -> %name%.resources.dll
            var resourceName = Path.GetFileNameWithoutExtension(outputAssembly) + ".resources.dll";

            var embeddedResources = new List<string>();
            _emptyDirectories.Clear();
            foreach (var ci in embeddedCultures)
            {
                var resourceFile = Path.Combine(workingDir, ci.Name, resourceName);
                if (File.Exists(resourceFile))
                {
                    embeddedResources.Add(ci.Name);
                    CleanupResource(resourceFile, ci);
                }
            }

            if (embeddedResources.Count == 1)
            {
                Logger.Info("Deleted resource file '{0}' as it was embedded into the target.", embeddedResources[0]);
            }
            else if (embeddedResources.Count > 1)
            {
                Logger.Info("Deleted resource files '{0}' as they where embedded into the target.", string.Join(", ", embeddedResources));
            }
            if (_emptyDirectories.Count == 1)
            {
                Logger.Info("Deleted resource directory '{0}' as it is empty.", _emptyDirectories[0]);
            }
            else if (_emptyDirectories.Count > 1)
            {
                Logger.Info("Deleted resource directories '{0}' as they are empty.", string.Join(", ", _emptyDirectories));
            }
            var notEmptyDirectories = embeddedResources.Except(_emptyDirectories).ToList();
            if (notEmptyDirectories.Count == 1)
            {
                Logger.Info("Resource directory '{0}' is not empty, thus will be kept.", notEmptyDirectories[0]);
            }
            else if (notEmptyDirectories.Count > 1)
            {
                Logger.Info("Resource directories '{0}' are not empty, thus will be kept.", string.Join(", ", notEmptyDirectories));
            }
            return true;
        }

        protected virtual void CleanupResource(string resourceFile, CultureInfo ci)
        {
            File.Delete(resourceFile);
            // check whether that was the last file of the specific language, if so -> delete the directory
            var dir = new FileInfo(resourceFile).DirectoryName;
            if (Directory.GetFileSystemEntries(dir).Length == 0)
            {
                // empty dir -> we just deleted the last resource from it, so delete it as well
                // retry on error in case build was not fully finished with copying files
                Retry<Exception>(() =>
                {
                    Directory.Delete(dir);
                    _emptyDirectories.Add(ci.Name);
                },
                ex =>
                {
                    // happens e.g. if user has directory locked via another application without any files in the directory
                    Logger.Warning("Failed to delete resource directory '{0}': {1}", ci.Name, ex.Message);
                });
            }
        }

        private static void Retry<TException>(Action action, Action<TException> onError, int tryCount = 1) where TException : Exception
        {
            while (tryCount-- > 0)
            {
                try
                {
                    action();
                }
                catch (TException e)
                {
                    if (tryCount == 0)
                        onError(e);
                }
            }
        }

        /// <summary>
        /// Returns the list of culture names that have been embedded into the assembly by <see cref="SatelliteAssemblyEmbedderTask"/>.
        /// </summary>
        /// <param name="outputAssembly"></param>
        /// <returns></returns>
        private string[] GetEmbeddedCultureNames()
        {
            // it's possible to have no cultures to be merged
            if (string.IsNullOrEmpty(EmbeddedCultures))
                return new string[0];

            return EmbeddedCultures.Contains(";") ? EmbeddedCultures.Split(';') : new[] { EmbeddedCultures };
        }

        /// <summary>
        /// For a given input file will find all embedded resources that have been embedded by <see cref="SatelliteAssemblyEmbedderTask"/>.
        /// </summary>
        /// <param name="outputAssembly"></param>
        /// <returns></returns>
        protected IEnumerable<CultureInfo> GetEmbeddedCultures()
        {
            var names = GetEmbeddedCultureNames();

            foreach (var ci in names)
            {
                CultureInfo culture;
                try
                {
                    culture = CultureInfo.GetCultureInfo(ci);
                }
                catch (Exception)
                {
                    // possible that user has embedded something with same name pattern, just ignore it
                    continue;
                }
                yield return culture;
            }
        }
    }
}
