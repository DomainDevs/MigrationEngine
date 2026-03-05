using Core.Entities;
using Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Engine.Services
{
    public class MigrationService
    {
        private readonly ILogWriterMD _logWriter;

        public MigrationService(ILogWriterMD logWriter)
        {
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        /// <summary>
        /// Ejecuta un paso individual y muestra resultado mínimo en consola
        /// </summary>
        private LogEntry EjecutarPaso(MigrationStep step)
        {
            step.Inicio = DateTime.Now;
            var logEntry = new LogEntry
            {
                NombrePaso = step.Nombre,
                Inicio = step.Inicio
            };

            try
            {
                // Aquí se ejecutaría el paquete SSIS
                step.Exito = true;
                step.Mensaje = "Paso ejecutado correctamente";

                logEntry.Exito = true;
                logEntry.Mensaje = step.Mensaje;

                // Consola mínima
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"> {step.Nombre} [OK]");
            }
            catch (Exception ex)
            {
                step.Exito = false;
                step.Mensaje = ex.Message;

                logEntry.Exito = false;
                logEntry.Mensaje = ex.Message;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"> {step.Nombre} [FAIL]");
            }
            finally
            {
                Console.ResetColor();
                step.Fin = DateTime.Now;
                logEntry.Fin = step.Fin;
            }

            return logEntry;
        }

        /// <summary>
        /// Ejecuta todos los pasos de un job
        /// </summary>
        public void EjecutarJob(MigrationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            if (job.Pasos == null || job.Pasos.Count == 0) return;

            job.FechaEjecucion = DateTime.Now;

            var logs = job.Pasos.Select(EjecutarPaso).ToList();
            job.Completado = job.Pasos.All(p => p.Exito);

            _logWriter.EscribirLog(job.Nombre, logs);

            // Generar reporte JSON
            GenerarReporteJson(job, logs);

            // Mostrar resumen final
            MostrarResumen(job, logs);
        }

        /// <summary>
        /// Ejecuta un job dinámico desde carpeta, filtrando paquetes si es necesario
        /// </summary>
        public void EjecutarJobDesdeCarpeta(string nombreJob, string carpetaPaquetes, List<string>? paquetesEspecificos = null)
        {
            if (string.IsNullOrWhiteSpace(nombreJob))
                throw new ArgumentException("Nombre del job requerido.", nameof(nombreJob));

            if (!Directory.Exists(carpetaPaquetes))
                throw new DirectoryNotFoundException($"La carpeta {carpetaPaquetes} no existe.");

            var archivosDisponibles = Directory.GetFiles(carpetaPaquetes, "*.dtsx");

            // Si se especificaron paquetes
            if (paquetesEspecificos != null && paquetesEspecificos.Count > 0)
            {
                var nombresDisponibles = archivosDisponibles
                    .Select(f => Path.GetFileName(f))
                    .ToList();

                // Detectar paquetes inexistentes
                var paquetesNoEncontrados = paquetesEspecificos
                    .Where(p => !nombresDisponibles.Contains(p, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (paquetesNoEncontrados.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[ERROR] Error de parametrización. Los siguientes paquetes no existen:");
                    foreach (var p in paquetesNoEncontrados)
                    {
                        Console.WriteLine($" - {p}");
                    }
                    Console.ResetColor();

                    throw new FileNotFoundException(
                        $"Paquetes no encontrados: {string.Join(", ", paquetesNoEncontrados)}"
                    );
                }

                // Filtrar solo los que existen
                archivosDisponibles = archivosDisponibles
                    .Where(f => paquetesEspecificos.Contains(Path.GetFileName(f), StringComparer.OrdinalIgnoreCase))
                    .ToArray();
            }

            if (archivosDisponibles.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR] No hay paquetes válidos para ejecutar.");
                Console.ResetColor();
                return;
            }

            var pasos = archivosDisponibles.Select(a => new MigrationStep
            {
                Nombre = Path.GetFileNameWithoutExtension(a),
                RutaPaquete = a
            }).ToList();

            var job = new MigrationJob
            {
                Nombre = nombreJob,
                Pasos = pasos,
                FechaEjecucion = DateTime.Now
            };

            EjecutarJob(job);
        }

        /// <summary>
        /// Muestra en consola un resumen del Job ejecutado
        /// </summary>
        private void MostrarResumen(MigrationJob job, List<LogEntry> logs)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n====== Resumen Job: {job.Nombre} ======");
            Console.ResetColor();

            int exitos = logs.Count(l => l.Exito);
            int fallidos = logs.Count - exitos;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Total pasos: {logs.Count}");
            Console.WriteLine($"Éxitos:      {exitos}");
            Console.WriteLine($"Fallidos:    {fallidos}");
            Console.ResetColor();

            // Estado final del job
            Console.ForegroundColor = job.Completado ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"\nEstado final del Job: {(job.Completado ? "[OK]" : "[FAIL]")}");
            Console.ResetColor();
        }

        /// <summary>
        /// Genera un reporte JSON completo del Job ejecutado
        /// </summary>
        private void GenerarReporteJson(MigrationJob job, List<LogEntry> logs)
        {
            try
            {
                var reporte = new
                {
                    job.Nombre,
                    FechaEjecucion = job.FechaEjecucion.HasValue
                    ? job.FechaEjecucion.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : null,
                    Completado = job.Completado,
                    TotalPasos = logs.Count,
                    Exitos = logs.Count(l => l.Exito),
                    Fallidos = logs.Count(l => !l.Exito),
                    Pasos = logs.Select(l => new
                    {
                        l.NombrePaso,
                        l.Inicio,
                        l.Fin,
                        l.Exito,
                        l.Mensaje
                    }).ToList()
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(reporte, jsonOptions);

                string rutaReporte = Path.Combine(AppContext.BaseDirectory, $"{job.Nombre}_MigrationReport.json");
                File.WriteAllText(rutaReporte, json);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nReporte JSON generado: {rutaReporte}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[WARN] No se pudo generar reporte JSON: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}