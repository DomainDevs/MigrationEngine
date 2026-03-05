using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace MigrationExecutor.Config
{
    internal static class Startup
    {
        private const string ConfigDirectory = "Config";

        internal static IHostBuilder AddConfigurations(this IHostBuilder host)
        {
            host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                IHostEnvironment env = hostingContext.HostingEnvironment;
                DirectoryInfo directory = new DirectoryInfo(ConfigDirectory);

                if (directory.Exists)
                {
                    foreach (FileInfo file in directory.EnumerateFiles("*.json"))
                    {
                        string baseName = Path.GetFileNameWithoutExtension(file.Name);
                        string extension = Path.GetExtension(file.Name);

                        // archivo base
                        config.AddJsonFile(
                            Path.Combine(ConfigDirectory, file.Name),
                            optional: false,
                            reloadOnChange: true);

                        // archivo específico por entorno
                        config.AddJsonFile(
                            Path.Combine(ConfigDirectory, $"{baseName}.{env.EnvironmentName}{extension}"),
                            optional: true,
                            reloadOnChange: true);
                    }
                }

                // agregar variables de entorno
                config.AddEnvironmentVariables();
            });

            return host;
        }
    }
}