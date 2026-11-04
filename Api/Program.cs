using Api.Services;
using Data;
using Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
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

var app = builder.Build();

app.UseExceptionHandler();
        
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
