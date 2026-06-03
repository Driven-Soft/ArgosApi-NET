using System.Text.Json.Serialization;
using Argos.Api.Exceptions;
using Argos.Api.Extensions;
using Argos.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    // Enums saem como string UPPER_SNAKE no JSON (ex.: "ALTO", "EM_ANALISE") — contrato do app.
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSwagger();
builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddDbContext(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.RoutePrefix = "swagger");

    // Seeds mínimos — idempotente, só roda em desenvolvimento.
    await ArgosSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();
