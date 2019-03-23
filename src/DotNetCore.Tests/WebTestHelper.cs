using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;
using WebDotNetCore;

namespace DotNetCore.Tests
{
    public static class WebTestHelper
    {
        /// <summary>
        /// Helper that sets up api server.
        /// </summary>
        /// <returns></returns>
        public static TestServer SetupTestServer(string relativePath)
        {
            var integrationTestsPath = PlatformServices.Default.Application.ApplicationBasePath;
            var applicationPath = Path.GetFullPath(Path.Combine(integrationTestsPath, relativePath));

            var testServer = new TestServer(
                Program.BuildWebHostBuilder<Startup>()
                .UseContentRoot(applicationPath)
                .UseEnvironment("Development"));

            // defaults to http which would cause only 302 redirect responses
            if (!testServer.BaseAddress.ToString().StartsWith("https:"))
                testServer.BaseAddress = new Uri("https" + testServer.BaseAddress.ToString().Substring("http".Length));
            return testServer;
        }
    }
}
