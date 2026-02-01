using Microsoft.EntityFrameworkCore;
using aletrail_api.DAL;
using System.IO;
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using aletrail_api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT settings and authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "ChangeThis_Default_ReplaceInProduction!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = !string.IsNullOrEmpty(jwtSection["Issuer"]),
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = !string.IsNullOrEmpty(jwtSection["Audience"]),
        ValidAudience = jwtSection["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// Determine DB connection: prefer explicit CONNECTION_STRING env var, then Postgres secret file, then fallback to DefaultConnection (sqlite)
var configuration = builder.Configuration;
string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
bool usePostgres = false;

// Prefer Postgres config from appsettings.Development.json when running in Development
if (builder.Environment.IsDevelopment())
{
    var pgConfig = configuration.GetSection("Postgres");
    var pgPassword = pgConfig["Password"];
    if (!string.IsNullOrEmpty(pgPassword))
    {
        var host = pgConfig["Host"] ?? "postgres";
        var db = pgConfig["Database"] ?? "aletrail";
        var user = pgConfig["User"] ?? "aletrail_user";
        connectionString = $"Host={host};Database={db};Username={user};Password={pgPassword}";
        usePostgres = true;
    }
}

// If not set from configuration, evaluate CONNECTION_STRING env var (may be a Postgres conn string)
if (!usePostgres && !string.IsNullOrEmpty(connectionString))
{
    // Quick heuristic: if it contains Host= or Server= it's likely a DB connection string (assume Postgres)
    if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
    {
        usePostgres = true;
    }
}

// If still not using Postgres and no explicit CONNECTION_STRING, attempt to read a password from a secret file (Docker secret)
if (!usePostgres && string.IsNullOrEmpty(connectionString))
{
    var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? configuration["Postgres:Host"] ?? "postgres";
    var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? configuration["Postgres:Database"] ?? "aletrail";
    var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? configuration["Postgres:User"] ?? "aletrail_user";
    var pgPasswordFile = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD_FILE") ?? "/run/secrets/postgres_password";

    if (File.Exists(pgPasswordFile))
    {
        var pwd = File.ReadAllText(pgPasswordFile).Trim();
        connectionString = $"Host={pgHost};Database={pgDb};Username={pgUser};Password={pwd}";
        usePostgres = true;
    }
}

// Final fallback to sqlite if nothing else provided
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = configuration.GetConnectionString("DefaultConnection");
}

// Register DbContext depending on provider
if (usePostgres)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();