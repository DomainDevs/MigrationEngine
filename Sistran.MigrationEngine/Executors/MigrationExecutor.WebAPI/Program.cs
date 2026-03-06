using Engine;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MigrationExecutor.WebAPI.Utils;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración desde appsettings.json
builder.Configuration.AddJsonFile("Configurations/appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Configurations/documentation.json", optional: false, reloadOnChange: true);

// Para poder inyectar directamente MigrationConfig, opcionalmente:
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MigrationConfig>>().Value
);

// Registrar servicios de Infrastructure y Engine
var rutaLogs = builder.Configuration.GetValue<string>("Migration:CarpetaLogs");
builder.Services.AddInfrastructureServices(builder.Configuration, rutaLogs, true);

builder.Services.AddEngineServices();

// Registrar controladores
builder.Services.AddControllers();

var app = builder.Build();

app.UseInfrastructure(builder.Configuration);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();