using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Cortex.Mediator.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mindflow_backend.Analytics.Application.Services;
using Mindflow_backend.Subscriptions.Application.Services;
using Mindflow_backend.Subscriptions.Infrastructure.Services;
using Mindflow_backend.Journal.Application.Services;
using Mindflow_backend.Journal.Infrastructure.Services;
using Mindflow_backend.Habits.Application.Internal.CommandServices;
using Mindflow_backend.Habits.Application.Internal.QueryServices;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Habits.Infrastructure.Persistence.Ef.Repositories;
using Mindflow_backend.iam.application.Internal.commandservices;
using Mindflow_backend.iam.application.services;
using Mindflow_backend.iam.domain.repositories;
using Mindflow_backend.iam.infrastructure.persistence.entityframeworkcore.repositories;
using Mindflow_backend.iam.infrastructure.services;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Mindflow_backend.Shared.Interfaces.Rest.ProblemDetails;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

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

var jwtSecret = builder.Configuration["TokenSettings:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MindFlow.API",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MindFlow.Frontend",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddLocalization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 5;
    });
    options.RejectionStatusCode = 429;
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<ProblemDetailsFactory>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();

builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IHabitCompletionLogRepository, HabitCompletionLogRepository>();
builder.Services.AddScoped<IHabitCommandService, HabitCommandService>();
builder.Services.AddScoped<IHabitLogCommandService, HabitLogCommandService>();
builder.Services.AddScoped<IHabitQueryService, HabitQueryService>();
builder.Services.AddScoped<IHabitLogQueryService, HabitLogQueryService>();

builder.Services.AddScoped<ISubscriptionService, StripeSubscriptionService>();
builder.Services.AddScoped<AnalyticsComputationService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCortexMediator([typeof(Program)]);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontendPolicy");
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("fixed");

app.Run();
