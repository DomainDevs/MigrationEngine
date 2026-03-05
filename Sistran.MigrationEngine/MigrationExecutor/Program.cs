using Engine;
using Engine.Services;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationExecutor.Utils;

try
{

    Console.WriteLine("Iniciando migración...");

    // Crear host y cargar configuración desde Config/
    var host = Host.CreateDefaultBuilder(args)
    .AddConfigurations() // carga todos los JSON de Config/ y variables de entorno
    .ConfigureServices((context, services) =>
    {
        // Leer sección Migration del JSON
        var migrationConfig = context.Configuration.GetSection("Migration").Get<MigrationConfig>();

        // Registrar servicios usando configuración
        services.AddInfrastructureServices(migrationConfig.CarpetaLogs);
        services.AddEngineServices();
    })
    .Build();

    // Resolver servicio principal
    var migrationService = host.Services.GetRequiredService<MigrationService>();

    // Obtener configuración de migration para ejecución
    var migrationConfigRoot = host.Services.GetRequiredService<IConfiguration>();
    var migrationConfigObj = migrationConfigRoot.GetSection("Migration").Get<MigrationConfig>();

    // Ejecutar job dinámico desde carpeta (ya encapsulado en MigrationService)
    Console.WriteLine("Ejecutando Paquetes...");
    migrationService.EjecutarJobDesdeCarpeta(
    migrationConfigObj.NombreJob,
    migrationConfigObj.CarpetaPaquetes
    );

    Console.WriteLine("Migración completada. Presione cualquier tecla para salir...");
    Console.ReadKey();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    Console.WriteLine("Error iniciando aplicación:", ex.Message);
}
finally
{
}




