using Engine.Services;
using Infrastructure;
using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Core.Entities;
using System.IO;

// Crear contenedor de servicios
var services = new ServiceCollection();

// Registrar Infrastructure
services.AddInfrastructureServices(@"C:\Temp\LogMigration");

// Registrar Engine con inyección de LogWriterMD
services.AddSingleton<MigrationService>();

var provider = services.BuildServiceProvider();

// Resolver servicios
var migrationService = provider.GetRequiredService<MigrationService>();
var logWriter = provider.GetRequiredService<LogWriterMD>();

// Carpeta donde están los paquetes .dtsx
string carpetaPaquetes = @"C:\Devs\Core\SISTRAN\MIG1\Sistran.MigrationEngine\MigracionSISE";

// Crear job dinámicamente leyendo los paquetes
var archivos = Directory.GetFiles(carpetaPaquetes, "*.dtsx");

var job = new MigrationJob
{
    Nombre = "MigracionDinamica",
    Pasos = archivos.Select(a => new MigrationStep
    {
        Nombre = Path.GetFileNameWithoutExtension(a),
        RutaPaquete = a
    }).ToList()
};

// Ejecutar job
migrationService.EjecutarJob(job);

// Mostrar resumen por consola
Console.WriteLine($"Job {job.Nombre} ejecutado. Completado: {job.Completado}");
foreach (var paso in job.Pasos)
{
    Console.WriteLine($"Paso: {paso.Nombre}, Éxito: {paso.Exito}, Mensaje: {paso.Mensaje}");
}

Console.WriteLine("Presione cualquier tecla para salir...");
Console.ReadKey();