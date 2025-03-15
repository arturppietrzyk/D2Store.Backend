using D2Store.Api.Config;
using D2Store.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

builder.Configuration
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("D2Store");

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var assembly = typeof(Program).Assembly;

builder.Services.Configure<ConnectionStringsConfig>(
    builder.Configuration.GetSection(ConnectionStringsConfig.SectionName));
var connectionStringsConfig = new ConnectionStringsConfig();
builder.Configuration.GetSection(ConnectionStringsConfig.SectionName).Bind(connectionStringsConfig);

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(o =>
o.UseSqlServer(connectionStringsConfig.D2Store));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
