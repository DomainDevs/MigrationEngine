using Engine;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MigrationExecutor.WebAPI.Utils;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración *.json
builder.Configuration.AddJsonFile("Configurations/appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Configurations/documentation.json", optional: false, reloadOnChange: true);

// Para poder inyectar directamente MigrationConfig, opcionalmente:
builder.Services.Configure<MigrationConfig>(builder.Configuration.GetSection("Migration"));

// Recupera carpeta de logs
string rutaLogs = builder.Configuration.GetValue<string>("Migration:CarpetaLogs");

builder.Services.AddInfrastructureServices(builder.Configuration, rutaLogs, true); //Registrar Infraestructura
builder.Services.AddEngineServices(); // Registrar Engine
builder.Services.AddControllers(); // Registrar controladores

var app = builder.Build();

app.UseInfrastructure(builder.Configuration);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();