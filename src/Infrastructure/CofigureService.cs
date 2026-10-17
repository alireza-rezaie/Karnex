using Application.Contracts;
using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public static class CofigureService
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
        //Connection String Sql Server
        services.AddDbContext<ApplicationDbContext>(option =>
        {
            option.UseSqlServer(configuration.GetConnectionString("Default"));
        });
        //Connection String Redis
        services.AddSingleton<IConnectionMultiplexer>(opt =>
        {
            var Option = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"), true);
            return ConnectionMultiplexer.Connect(Option);
        });
        //DI
        //services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, TokenService>();

        //Identity
        services.AddIdentityService(configuration);
        return services;
        }

    }
