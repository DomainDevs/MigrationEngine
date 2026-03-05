using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure
{
    public static class AddInfrastructureExtension
    {
        /// <summary>
        /// Registra los servicios de Infrastructure en el contenedor DI
        /// </summary>
        /// <param name="services">Contenedor de servicios</param>
        /// <param name="rutaLogs">Ruta base donde se generarán los archivos MD</param>
        /// <returns>El contenedor de servicios actualizado</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string rutaLogs)
        {
            if (string.IsNullOrWhiteSpace(rutaLogs))
                throw new ArgumentException("Debe indicar la ruta base para los logs.", nameof(rutaLogs));

            // Registramos LogWriterMD como singleton
            services.AddSingleton(new LogWriterMD(rutaLogs));

            return services;
        }
    }
}