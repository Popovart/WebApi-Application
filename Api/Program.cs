using Api.Services;
using Data;
using Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.Services;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddLogging();

var dataSource = builder.Configuration["DataSource"] ?? "Sql";
switch (dataSource)
{
    case "Sql":
        builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
        builder.Services.AddDbContext<LocalDbContext>(options =>
        {
            var connectionString =builder.Configuration.GetConnectionString("ProductsDb");
            options.UseSqlServer(connectionString);
        });
        break;

    case "Mock":
        builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        break;

    default:
        throw new InvalidOperationException($"Unsupported DataSource value '{dataSource}'. Supported values are 'Sql' and 'Mock'.");
}

builder.Services
    .AddApiVersioning(options =>
    {
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    })
    .AddOpenApi(options =>
    {
        options.Document.ShouldInclude = description =>
            description.GroupName == options.Description.GroupName;
    });

var app = builder.Build();

app.UseExceptionHandler();
        
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
        options.SwaggerEndpoint("/openapi/v2.json", "API v2");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
