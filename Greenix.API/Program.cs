using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Greenix.Persistence;
using Greenix.Infrastructure;
using Greenix.Application;
using Greenix.API.Middlewares;
using Greenix.Domain.Entities;
using Greenix.Infrastructure.Identity;

// Setup Bootstrap Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Greenix API starting up...");

    var builder = WebApplication.CreateBuilder(args);

    // 1. Serilog Setup
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // 2. Register Controllers
    builder.Services.AddControllers();

    // 3. Register Health Checks
    builder.Services.AddHealthChecks();

    // 4. Register API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

    // 5. Register Application Layer Services
    builder.Services.AddApplicationServices();

    // 6. Register Persistence Layer
    builder.Services.AddPersistenceServices(builder.Configuration);

    // 7. Register Infrastructure Layer
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // 8. Configure JWT Authentication
    var jwtSettingsSecret = builder.Configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JWT Secret not configured.");

    var jwtSettingsIssuer = builder.Configuration["JwtSettings:Issuer"];
    var jwtSettingsAudience = builder.Configuration["JwtSettings:Audience"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettingsIssuer,
            ValidAudience = jwtSettingsAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettingsSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

    // 9. Register Swagger
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Greenix Civic Management API",
            Description = "Enterprise REST services for Smart Civic Complaint Management"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {your JWT token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // 10. Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultCorsPolicy", policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("http://localhost:3000")
                  .AllowCredentials();
        });
    });

    var app = builder.Build();

    // ================================
    // Seed Roles & Default Administrator
    // ================================
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await IdentitySeeder.SeedAsync(userManager, roleManager);
    }

    // Configure HTTP pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Greenix API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseCors("DefaultCorsPolicy");

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly during start-up.");
}
finally
{
    Log.CloseAndFlush();
}