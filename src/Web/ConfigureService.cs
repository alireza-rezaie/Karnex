using Application.Contracts;
using Domain.Exceptions;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Web.Extensions;
using Web.Middleware;
using Web.Services;

namespace Web;

static class ConfigureService
{
    public static IServiceCollection AddWebConfigureServices(this WebApplicationBuilder builder,
        IConfiguration configuration)
    {
        // Require an authenticated principal for every endpoint unless it is
        // explicitly marked with [AllowAnonymous]. This prevents newly added
        // controllers/actions from accidentally becoming public.
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        builder.Services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerDocumentation();

        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(
                    "http://localhost:4200", "https://localhost:4200",
                    "http://localhost:5173", "https://localhost:5173",
                    "http://localhost:4173", "https://localhost:4173",
                    "http://localhost:3000", "https://localhost:3000");
            });
        });

        builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        return builder.Services;
    }

    private static void ApiBehaviorOptions(WebApplicationBuilder builder)
    {
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                var errors = actionContext.ModelState
                    .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                    .SelectMany(v => v.Value!.Errors)
                    .Select(c => c.ErrorMessage).ToList();

                return new BadRequestObjectResult(new ApiToReturn(400, errors));
            };
        });
    }

    public static async Task<IApplicationBuilder> AddWebAppService(this WebApplication app)
    {
        app.UseMiddleware<MiddlewareExceptionHandler>();

        var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            await GenerateFakeData.SeedDataAsync(context, loggerFactory);
        }
        catch (Exception e)
        {
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(e, "Error Exception For Migration");
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerDocumentation();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
        return app;
    }
}