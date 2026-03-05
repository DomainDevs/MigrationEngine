using Core.Entities;
using Infrastructure.Logging;
using System;
using System.Collections.Generic;

namespace Engine.Services
{
    public class MigrationService
    {
        private readonly LogWriterMD _logWriter;
        private readonly List<LogEntry> _logTemp = new();

        public MigrationService(LogWriterMD logWriter)
        {
            _logWriter = logWriter;
        }

        public void EjecutarPaso(MigrationStep step)
        {
            step.Inicio = DateTime.Now;

            try
            {
                // Aquí ejecutarías el paquete SSIS
                step.Exito = true;
                step.Mensaje = "Paso ejecutado correctamente";
            }
            catch (Exception ex)
            {
                step.Exito = false;
                step.Mensaje = ex.Message;
            }
            finally
            {
                step.Fin = DateTime.Now;

                // Guardar en lista temporal
                _logTemp.Add(new LogEntry
                {
                    NombrePaso = step.Nombre,
                    Exito = step.Exito,
                    Inicio = step.Inicio,
                    Fin = step.Fin,
                    Mensaje = step.Mensaje
                });
            }
        }

        public void EjecutarJob(MigrationJob job)
        {
            job.FechaEjecucion = DateTime.Now;

            foreach (var paso in job.Pasos)
            {
                EjecutarPaso(paso);
            }

            job.Completado = job.Pasos.TrueForAll(p => p.Exito);

            // Al final del Job, escribimos todo el log de una vez
            _logWriter.EscribirLog(job.Nombre, _logTemp);
        }
    }
}