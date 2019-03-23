using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebDotNetCore
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(params string[] args) => BuildWebHostBuilder<Startup>(args).Build();

        /// <summary>
        /// Indirection for integration tests. Allows override with custom class that inherits Startup
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder BuildWebHostBuilder<T>(params string[] args) where T : Startup
        {
            var hostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseStartup<T>();
            return hostBuilder;
        }
    }
}
