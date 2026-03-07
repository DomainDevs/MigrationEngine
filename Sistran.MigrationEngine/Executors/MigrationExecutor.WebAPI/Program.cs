using Engine;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MigrationExecutor.WebAPI.Utils;
using Infrastructure.Documentation; // <- para AddConfiguredSwagger

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración *.json
builder.Configuration.AddJsonFile("Configurations/appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Configurations/documentation.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Configurations/cors.json", optional: false, reloadOnChange: true);

// Para poder inyectar directamente MigrationConfig, opcionalmente:
builder.Services.Configure<MigrationConfig>(builder.Configuration.GetSection("Migration"));

// Recupera carpeta de logs
string basePath = AppContext.BaseDirectory; // ruta del exe o WebAPI

string rutaLogs = Path.Combine(basePath, builder.Configuration.GetValue<string>("Migration:CarpetaLogs"));

builder.Services.AddInfrastructureServices(builder.Configuration, rutaLogs, true); //Registrar Infraestructura
builder.Services.AddEngineServices(); // Registrar Engine
//builder.Services.AddControllers(); // Registrar controladores

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseInfrastructure(builder.Configuration);

app.MapControllers();

app.Run();