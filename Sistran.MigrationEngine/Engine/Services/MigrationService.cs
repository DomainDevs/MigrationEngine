using Core.Entities;
using Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

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
        /// Ejecuta un paso individual y devuelve el LogEntry
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
            }
            catch (Exception ex)
            {
                step.Exito = false;
                step.Mensaje = ex.Message;

                logEntry.Exito = false;
                logEntry.Mensaje = ex.Message;
            }
            finally
            {
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

            // Escribir log al final
            _logWriter.EscribirLog(job.Nombre, logs);
        }
    }
}