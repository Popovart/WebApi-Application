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
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddDbContext<LocalDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ProductsDb");
    options.UseSqlServer(connectionString);
});
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
        
// Configure the HTTP request pipeline.
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
