using Services.Interfaces;
using Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddTransient<IProductService, ProductService>();

var app = builder.Build();
        
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
