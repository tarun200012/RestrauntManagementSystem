using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Repositories;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;
using RestaurantAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();

builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<ILocationService, LocationService>();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Optional

var app = builder.Build();

// Enable Swagger middleware only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // You can also use options like setting a custom endpoint
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
