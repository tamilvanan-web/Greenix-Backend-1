using Microsoft.EntityFrameworkCore;
using Greenix.Persistence.Context;
using Greenix.API.Middlewares;
using Greenix.Application;
using Greenix.Domain.Entities;
using Greenix.Infrastructure;
using Greenix.Infrastructure.Identity;
using Greenix.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;

// Bootstrap Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Greenix API starting up...");

    var builder = WebApplication.CreateBuilder(args);

    Console.WriteLine("===== DATABASE CONNECTION =====");
    Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));
    Console.WriteLine("===============================");


    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());


    // Controllers
    builder.Services.AddControllers();


    // Health Checks
    builder.Services.AddHealthChecks();


    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });


    // Application Layer
    builder.Services.AddApplicationServices();


    // Persistence Layer
    builder.Services.AddPersistenceServices(builder.Configuration);


    // Infrastructure Layer
    builder.Services.AddInfrastructureServices(builder.Configuration);



    // JWT Configuration
    var jwtSettingsSecret =
        builder.Configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JWT Secret not configured.");

    var jwtSettingsIssuer =
        builder.Configuration["JwtSettings:Issuer"];

    var jwtSettingsAudience =
        builder.Configuration["JwtSettings:Audience"];


    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettingsIssuer,
                ValidAudience = jwtSettingsAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettingsSecret)),

                ClockSkew = TimeSpan.Zero
            };
    });



    // Swagger
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1",
            new OpenApiInfo
            {
                Version = "v1",
                Title = "Greenix Civic Management API",
                Description =
                "Enterprise REST services for Smart Civic Complaint Management"
            });


        options.AddSecurityDefinition("Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                "Enter: Bearer {your JWT token}"
            });


        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },

                    Array.Empty<string>()
                }
            });
    });



    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultCorsPolicy",
            policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(
                        "http://localhost:3000",
                        "http://localhost:5173")
                    .AllowCredentials();
            });
    });



    var app = builder.Build();



    // ===============================
    // Database Migration + Identity Seed
    // ===============================

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var dbContext =
            services.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();


        var userManager =
            services.GetRequiredService<UserManager<User>>();

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();


        await IdentitySeeder.SeedAsync(
            userManager,
            roleManager);
    }




    // ===============================
    // Swagger (Production Enabled)
    // ===============================

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Greenix API v1");

        options.RoutePrefix = "swagger";
    });



    // Middleware Pipeline

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseCors("DefaultCorsPolicy");

    app.UseAuthentication();

    app.UseAuthorization();



    // Controllers

    app.MapControllers();



    // Health Endpoint

    app.MapHealthChecks("/health");



    // Root Endpoint

    app.MapGet("/", () =>
    {
        return "Greenix API is running successfully ??";
    });



    // Render Port

    var port =
        Environment.GetEnvironmentVariable("PORT");

    if (!string.IsNullOrEmpty(port))
    {
        app.Urls.Add($"http://*:{port}");
    }



    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "Application terminated unexpectedly during start-up.");
}
finally
{
    Log.CloseAndFlush();
}