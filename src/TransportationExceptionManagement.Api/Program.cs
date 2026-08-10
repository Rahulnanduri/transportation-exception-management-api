using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using TransportationExceptionManagement.Api.ErrorHandling;
using TransportationExceptionManagement.Application.Cases;
using TransportationExceptionManagement.Application.Exports;
using TransportationExceptionManagement.Application.Reports;
using TransportationExceptionManagement.Infrastructure;
using TransportationExceptionManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false));
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Request validation failed",
            Type = "https://httpstatuses.com/400",
            Instance = context.HttpContext.Request.Path
        };

        problem.Extensions["code"] = "validation_failed";
        problem.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem);
    };
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd(
            "traceId",
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Transportation Exception Management API",
        Version = "v1",
        Description =
            "Independent portfolio API using deterministic synthetic transportation data and illustrative workflow rules."
    });
    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<ICaseCsvExportService, CaseCsvExportService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("Health")
    .WithTags("Health")
    .Produces(StatusCodes.Status200OK);

app.MapControllers();

await DatabaseInitializer.InitializeAsync(app.Services);
await app.RunAsync();

public partial class Program;
