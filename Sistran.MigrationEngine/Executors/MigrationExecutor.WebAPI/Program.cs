using Infrastructure;
using Engine;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración desde appsettings.json
builder.Configuration.AddJsonFile("Configurations/appsettings.json", optional: false, reloadOnChange: true);

// Registrar servicios de Infrastructure y Engine
var rutaLogs = builder.Configuration.GetValue<string>("Migration:CarpetaLogs");
builder.Services.AddInfrastructureServices(builder.Configuration, rutaLogs);
builder.Services.AddEngineServices();

// Registrar controladores
builder.Services.AddControllers();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MigrationExecutor API v1");
        c.RoutePrefix = string.Empty; // Para que abra en la raíz: https://localhost:5001/
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();