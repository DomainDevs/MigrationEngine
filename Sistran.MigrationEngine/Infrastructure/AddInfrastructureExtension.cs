using Infrastructure.Documentation;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config, string rutaLogs, Boolean isWEPApi = false)
        {
            if (string.IsNullOrWhiteSpace(rutaLogs))
                throw new ArgumentException("Debe indicar la ruta base para los logs.", nameof(rutaLogs));

            // Registrar LogWriterMD e ILogWriterMD
            var mdWriter = new LogWriterMD(rutaLogs);
            services.AddSingleton(mdWriter);
            services.AddSingleton<ILogWriterMD>(mdWriter);

            // Registrar LogWriterJSON e ILogWriterJSON
            var jsonWriter = new LogWriterJSON(rutaLogs);
            services.AddSingleton(jsonWriter);
            services.AddSingleton<ILogWriterJSON>(jsonWriter);

            if (isWEPApi)
            services.AddOpenApiDocumentation(config);


            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
            builder.UseOpenApiDocumentation(config);
    }
}