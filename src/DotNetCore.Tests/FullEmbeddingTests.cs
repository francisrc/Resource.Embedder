using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace DotNetCore.Tests
{
    [TestClass]
    public class FullEmbeddingTests
    {
        [TestMethod]
        public async Task NetCoreWebProjectShouldWorkWithEmbeddedResources()
        {
            const string rel = "../../../../testmodules/WebDotNetCore";
            var configuration = new DirectoryInfo(".").Parent.Name;
            var srcDir = Path.Combine(rel, $"bin/{configuration}/netcoreapp2.2");
            // ensure we have the right dir
            File.Exists(Path.Combine(srcDir, "WebDotNetCore.dll")).Should().BeTrue();
            var translationFolders = Directory.GetDirectories(srcDir);
            translationFolders.Should().HaveCount(0, "because they where embedded into the web project");

            using (var server = WebTestHelper.SetupTestServer(rel))
            using (var client = server.CreateClient())
            {
                async Task<string> SendAsync(string iso)
                {
                    var response = await client.GetAsync("?language=" + iso);
                    return await response.Content.ReadAsStringAsync();
                }
                var content = await SendAsync("en");
                content.Should().StartWith("Current culture: English");

                content = await SendAsync("de");
                content.Should().StartWith("Current culture: German");

                // fallback
                content = await SendAsync("en");
                content.Should().StartWith("Current culture: English");
            }
        }
    }
}
