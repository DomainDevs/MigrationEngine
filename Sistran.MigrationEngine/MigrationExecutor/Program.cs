using Engine;
using Engine.Services;
using Infrastructure;
using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Core.Entities;

// Crear contenedor DI (inyección)
var services = new ServiceCollection();

services.AddInfrastructureServices(@"C:\Temp\LogMigration"); // Registrar Infrastructure
services.AddEngineServices(); // Registrar Engine

var provider = services.BuildServiceProvider();

// Resolver servicio
var migrationService = provider.GetRequiredService<MigrationService>();

// Carpeta donde están los paquetes .dtsx
string carpetaPaquetes = @"C:\Devs\Core\SISTRAN\MIG1\Sistran.MigrationEngine\MigracionSISE";

// Leer todos los paquetes .dtsx
var archivos = Directory.GetFiles(carpetaPaquetes, "*.dtsx");

var pasos = archivos.Select(a => new MigrationStep
{
    Nombre = Path.GetFileNameWithoutExtension(a),
    RutaPaquete = a
}).ToList();

var job = new MigrationJob
{
    Nombre = "MigracionDinamica",
    Pasos = pasos
};

// Ejecutar job
migrationService.EjecutarJob(job);

Console.WriteLine($"Job {job.Nombre} ejecutado. Completado: {job.Completado}");
foreach (var paso in job.Pasos)
{
    Console.WriteLine($"Paso: {paso.Nombre}, Éxito: {paso.Exito}, Mensaje: {paso.Mensaje}");
}

Console.WriteLine("Presione cualquier tecla para salir...");
Console.ReadKey();