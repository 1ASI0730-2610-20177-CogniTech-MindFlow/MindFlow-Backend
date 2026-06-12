using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Cortex.Mediator.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. RUTAS Y CONTROLADORES
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. CONFIGURACIÓN CORS (Esencial para la comunicación fluida con la interfaz en Vite)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. BASE DE DATOS (MySQL)
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Connection string not set.");
    
    options.UseMySQL(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .EnableDetailedErrors();
        
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// 4. INYECCIÓN DE DEPENDENCIAS Y CQRS
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registrar el bus de Mediator para procesar los Commands/Queries de los distintos Bounded Contexts
builder.Services.AddCortexMediator([typeof(Program)]);

var app = builder.Build();

// 5. MIGRACIONES AUTOMÁTICAS
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

// 6. PIPELINE DE PETICIONES HTTP
// Inyectar tu interceptor de errores (Asegúrate de tener el método de extensión correspondiente)
// app.UseGlobalExceptionHandler(); 

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontendPolicy"); // Activar CORS antes de la autorización

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();