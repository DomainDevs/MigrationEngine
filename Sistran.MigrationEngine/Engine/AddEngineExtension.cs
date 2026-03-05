using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Engine;

public static class AddEngineExtension
{
    /// <summary>
    /// Registra los servicios de Engine en el contenedor DI
    /// </summary>
    /// <param name="services">Contenedor de servicios</param>
    /// <returns>El contenedor de servicios actualizado</returns>
    public static IServiceCollection AddEngineServices(this IServiceCollection services)
    {
        // Registramos MigrationService
        // Usamos factory para inyectar LogWriterMD desde el contenedor
        services.AddSingleton<Services.MigrationService>(sp =>
        {
            var logWriter = sp.GetRequiredService<LogWriterMD>();
            return new Services.MigrationService(logWriter);
        });

        return services;
    }
}