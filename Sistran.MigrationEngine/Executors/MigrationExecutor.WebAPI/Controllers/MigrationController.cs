using Microsoft.AspNetCore.Mvc;
using Engine.Services;
using Core.Entities;
using MigrationExecutor.WebAPI.Utils;

namespace MigrationExecutor.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MigrationController : ControllerBase
{
    private readonly MigrationService _migrationService;
    private readonly MigrationConfig _config;

    public MigrationController(MigrationService migrationService, MigrationConfig config)
    {
        _migrationService = migrationService ?? throw new ArgumentNullException(nameof(migrationService));
        _config = config;
    }


    [HttpPost("run-job-from-folderm")]
    public IActionResult EjecutarJobDesdeCarpeta([FromBody] JobRequest request)
    {
        try
        {
            _migrationService.EjecutarJobDesdeCarpeta(
                nombreJob: request.Nombre,
                carpetaPaquetes: _config.CarpetaPaquetes,
                paquetesIncluir: request.PaquetesIncluir,
                paquetesOmitir: request.PaquetesOmitir
            );

            return Ok(new { Mensaje = "Job ejecutado correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Ejecuta un job completo enviando la lista de pasos
    /// </summary>
    [HttpPost("run-job")]
    public IActionResult RunJob([FromBody] MigrationJob job)
    {
        if (job == null || job.Pasos == null || !job.Pasos.Any())
            return BadRequest("Debe indicar el job y sus pasos.");

        try
        {
            _migrationService.EjecutarJob(job);
            return Ok(new
            {
                Mensaje = "Job ejecutado correctamente",
                Job = job
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Mensaje = "Error al ejecutar el job",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Ejecuta un job desde carpeta, filtrando paquetes a incluir o excluir
    /// </summary>
    [HttpPost("run-job-from-folder")]
    public IActionResult RunJobFromFolder([FromQuery] string nombreJob,
                                          [FromQuery] string carpetaPaquetes,
                                          [FromQuery] string[] paquetesIncluir,
                                          [FromQuery] string[] paquetesOmitir)
    {
        if (string.IsNullOrWhiteSpace(nombreJob) || string.IsNullOrWhiteSpace(carpetaPaquetes))
            return BadRequest("Debe indicar el nombre del job y la carpeta de paquetes.");

        try
        {
            _migrationService.EjecutarJobDesdeCarpeta(
                nombreJob,
                carpetaPaquetes,
                paquetesIncluir?.ToList(),
                paquetesOmitir?.ToList()
            );

            return Ok(new { Mensaje = $"Job '{nombreJob}' ejecutado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Mensaje = "Error al ejecutar el job",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Endpoint de prueba para verificar que la API funciona
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("MigrationExecutor WebAPI activa.");
}


public class JobRequest
{
    public string Nombre { get; set; } = string.Empty;
    public List<string>? PaquetesIncluir { get; set; }
    public List<string>? PaquetesOmitir { get; set; }
}