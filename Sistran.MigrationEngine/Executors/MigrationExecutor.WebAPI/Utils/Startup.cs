
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;
using Engine;

namespace MigrationExecutor.WebAPI.Utils;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Carpeta de logs
        var rutaLogs = configuration.GetValue<string>("Migration:CarpetaLogs");

        // Infrastructure
        services.AddInfrastructureServices(rutaLogs);

        // Engine
        services.AddEngineServices();

        // Controllers
        services.AddControllers();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}
