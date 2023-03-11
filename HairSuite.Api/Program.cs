using System.Text.Json.Serialization;
using HairSuite;
using HairSuite.Domain;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Marten")
                           ?? throw new Exception("Could not find the connection string for Marten.");
    options.Connection(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.AutoCreateSchemaObjects = AutoCreate.All;
    }

    options.UseDefaultSerialization(EnumStorage.AsString);
    options.Projections.SelfAggregate<Reservation>();
}).AddAsyncDaemon(DaemonMode.Solo);

builder.Services.AddScoped<IApplicationService<Reservation>, ReservationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
