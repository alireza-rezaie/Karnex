using Web;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Diagnostics;
using Web.Middleware;

var builder = WebApplication.CreateBuilder(args);


//configuration
builder.Services.AddApllicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.AddWebConfigureServices(builder.Configuration);
var app = builder.Build();

await app.AddWebAppService().ConfigureAwait(false);
