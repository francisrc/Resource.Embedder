using System;
using System.Diagnostics;

namespace ResourceEmbedder.MsBuild
{
	/// <summary>
	/// Since <see cref="SatelliteAssemblyEmbedderTask"/> only embedds the files,
	/// but doesn't prevent them from getting deployed we will have to do so manually.
	/// </summary>
	public class SatelliteAssemblyCleanupTask : MsBuildTask
	{
		#region Methods

		public override bool Execute()
		{
			var logger = new MSBuildBasedLogger(BuildEngine, "ResourceEmbedder");
			if (SignAssembly)
			{
				// TODO: check required steps to add this feature
				logger.Error("Signed assemblies have not been implemented yet.");
				return false;
			}
			if (!AssertSetup(logger))
			{
				return false;
			}

			var watch = new Stopwatch();
			watch.Start();
			logger.Info("Beginning resource cleanup.");
			logger.Indent(1);

			throw new NotImplementedException();

			watch.Stop();
			logger.Info("Finished cleanup in {0}ms", watch.ElapsedMilliseconds);
			return true;
		}

		#endregion Methods
	}
}