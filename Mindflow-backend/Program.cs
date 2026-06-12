using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Cortex.Mediator.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddCortexMediator([typeof(Program)]);

var app = builder.Build();

// 5. MIGRACIONES AUTOMÁTICAS
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontendPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();