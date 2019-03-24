using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ResourceEmbedder.MsBuild
{
    /// <summary>
    /// Helper task to prevent the satellite assemblies from being published when using dotnet publish.
    /// This is needed because publish does not happen from output directory (where <see cref="SatelliteAssemblyCleanupTask"/> would work)
    /// but pulls directly from the obj directory again.
    /// </summary>
    public class SatelliteAssemblyCleanupPublishTask : SatelliteAssemblyCleanupTask
    {
        private List<ITaskItem> _filesToBePublished;

        public SatelliteAssemblyCleanupPublishTask()
            : base("ResourceEmbedder.CleanupPublish")
        {
        }

        [Required]
        public ITaskItem[] ExistingPublishPath { get; set; }

        [Required]
        public string PublishDir { get; set; }

        public override bool Execute()
        {
            if (!AssertSetup(Logger))
            {
                return false;
            }

            Logger.Info("Cleaning up resource files before publish");
            _filesToBePublished = ExistingPublishPath.ToList();
            var initialCount = _filesToBePublished.Count;

            var result = RunCleanup(PublishDir);

            var afterCount = _filesToBePublished.Count;
            var delta = initialCount - afterCount;
            if (delta > 0)
            {
                if (delta == 1)
                    Logger.Info("1 resource file was removed from publish");
                else
                    Logger.Info($"{initialCount - afterCount} resource files where removed from publish");
            }
            return result;
        }

        protected override void CleanupResource(string resourceFile, CultureInfo ci)
        {
            // logic only tested with "dotnet publish", other publish methods (WebDeploy, FileSytemPublish, ..) probably won't work
            var fn = new FileInfo(resourceFile).Name;
            var id = $"{ci.Name}\\{fn}";
            try
            {
                var publishedFile = Path.Combine(Path.Combine(ProjectDirectory, PublishDir), id);
                var toRemove = _filesToBePublished.FirstOrDefault(i => i.GetMetadata("TargetPath") == id);
                if (toRemove != null)
                {
                    _filesToBePublished.Remove(toRemove);
                    Logger.Info($"Removing {toRemove} from publish");
                    base.CleanupResource(publishedFile, ci);
                }
                else
                {
                    Logger.Warning($"Could not locate resource file {id}. File will probably be published.");
                }
            }
            catch (Exception e)
            {
                Logger.Warning($"Could not locate resource file {id}. File will probably be published. Error: {e}");
            }
        }
    }
}
