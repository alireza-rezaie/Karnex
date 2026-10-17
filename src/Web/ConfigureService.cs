using Application.Contracts;
using Domain.Exceptions;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.SeedData;
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
        // Add services to the container.

        builder.Services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        //ApiBehaviorOptions(builder);
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerDocumentation();
        //Corss Policy
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
        //I Http Contex IAccessor
        builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
        builder.Services.AddHttpContextAccessor();
        //Cache Memory
        builder.Services.AddDistributedMemoryCache();
        return builder.Services;
    }

    private static void ApiBehaviorOptions(WebApplicationBuilder builder)
    {
        //TODO check this
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


        //Create Scoped
        var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        //get services
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        //Auto migration
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

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            //app.UseSwagger();
            //app.UseSwaggerUI();
            app.UseSwaggerDocumentation();

        }
        //access to wwwroot
        app.UseStaticFiles();
        app.UseRouting();
        //Cors
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        await app.RunAsync();
        return app;
    }
}

