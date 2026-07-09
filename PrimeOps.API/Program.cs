using PrimeOps.LOGGER.Extensions;
using PrimeOps.PYTHON.Extensions;
using PrimeOps.PYTHON.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddFileLogger (builder.Configuration);

// Register the Python service
builder.Services.AddPythonService(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
