using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OrderTracker.Api;
using OrderTracker.Api.Data;
using OrderTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var useInMemory = builder.Configuration.GetValue("Database:UseInMemory", false);
var connectionString = builder.Configuration.GetConnectionString("KitchenDb")
    ?? "Server=(localdb)\\mssqllocaldb;Database=NdalamaKitchen;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useInMemory)
        options.UseInMemoryDatabase("NdalamaKitchen");
    else
        options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<OrderStore>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    MenuData.Seed(db);
}

app.UseExceptionHandler();
app.UseCors();
app.MapControllers();
app.Run();
