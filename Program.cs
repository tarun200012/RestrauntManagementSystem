using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Repositories;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;
using RestaurantAPI.Services;
using NLog;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hangfire.SqlServer;
using Hangfire;
using System.Security.Cryptography; // add this at the top of Program.cs
using System.Text;                  // add this too



var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
logger.Debug("init main");

try
{
    // --- One-time RSA key generation (DEV only) ---
    //using (var rsa = RSA.Create(2048))
    //{
    //    var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
    //    var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

    //    Console.WriteLine("========== RSA KEYS ==========");
    //    Console.WriteLine("Private Key (save this in appsettings.json):");
    //    Console.WriteLine(privateKey);
    //    Console.WriteLine();
    //    Console.WriteLine("Public Key (give this to Angular client):");
    //    Console.WriteLine(publicKey);
    //    Console.WriteLine("==============================");
    //    Console.ReadLine(); // Pause to allow copying the keys
    //}
    // --- End one-time generation ---


    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with NLog
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    builder.Host.UseNLog();

    // Register DbContext with DI container
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
    builder.Services.AddScoped<ICouponRepository, CouponRepository>();
    builder.Services.AddScoped<ILocationRepository, LocationRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<IRestaurantService, RestaurantService>();
    builder.Services.AddScoped<ILocationService, LocationService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
    builder.Services.AddScoped<ICouponService, CouponService>();
    builder.Services.AddScoped<IAuthRepository, AuthRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IJwtService, JwtService>();



    // ✅ Add CORS
    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
            builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                        .AllowCredentials();
            });
    });
    builder.Services.AddSingleton<RsaKeyService>();



    builder.Services.AddAutoMapper(typeof(Program));

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

    builder.Services.AddHangfire(configuration => configuration
       .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
       {
           QueuePollInterval = TimeSpan.Zero,
           UseRecommendedIsolationLevel = true
       }));



    // Add the Hangfire server
    builder.Services.AddHangfireServer();


    //RecurringJob.AddOrUpdate<ICouponService>(
    //   "CreateAutomaticCouponsJob",
    //   service => service.CreateAutomaticCoupons(null),
    //   Cron.Minutely);

    //Above is wrong way as we are registering a job very early when DI container is not ready

    //using (var scope = app.Services.CreateScope())
    //{
    //    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    //    var couponService = scope.ServiceProvider.GetRequiredService<ICouponService>();
    //    recurringJobManager.AddOrUpdate(
    //        "CreateAutomaticCouponsJob",
    //        () => couponService.CreateAutomaticCoupons(null),
    //        Cron.Minutely);
    //}


        // Add controllers
        builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(); // Optional

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var couponService = scope.ServiceProvider.GetRequiredService<ICouponService>();
        recurringJobManager.AddOrUpdate(
            "CreateAutomaticCouponsJob",
            () => couponService.CreateAutomaticCoupons(null),
            "0 0 1 * *");
    }


    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(); // You can also use options like setting a custom endpoint
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();

    // ✅ Enable CORS
    app.UseCors(MyAllowSpecificOrigins);

    app.UseAuthentication();

    //app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.UseHangfireDashboard();
    app.Run();

}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program due to exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown(); // Ensure all logs are flushed
}
