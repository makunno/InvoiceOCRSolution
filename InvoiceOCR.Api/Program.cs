using InvoiceOCR.Core.Services;
using InvoiceOCR.Data.Repositories;
using Microsoft.OpenApi.Models;
 // <- Add this

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddScoped<OcrService>();
builder.Services.AddScoped<InvoiceParser>();
builder.Services.AddScoped<InvoiceRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger registration for .NET 7
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InvoiceOCR API",
        Version = "v1",
        Description = "API for OCR-based Invoice Processing"
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvoiceOCR API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
