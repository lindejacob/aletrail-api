using Microsoft.EntityFrameworkCore;
using aletrail_api.DAL;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using aletrail_api.Models;
using aletrail_api.Services.Auth;
using aletrail_api.Services.Jwt;
using aletrail_api.Services.Security;
using aletrail_api.DAL.Account;
using aletrail_api.Services.PointOfInterest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<aletrail_api.Services.PubCrawl.IRouteCalculationService, aletrail_api.Services.PubCrawl.RouteCalculationService>();
builder.Services.AddScoped<aletrail_api.Services.PubCrawl.IPubCrawlService, aletrail_api.Services.PubCrawl.PubCrawlService>();

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

// Configure PostgreSQL database connection
var configuration = builder.Configuration;
string connectionString;

// Prefer explicit CONNECTION_STRING environment variable (for production/docker)
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONNECTION_STRING")))
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")!;
}
// For development, build connection string from config, using localhost for Postgres host
else if (builder.Environment.IsDevelopment())
{
    var pgConfig = configuration.GetSection("Postgres");
    var pgHost = "localhost"; // Always use localhost in development
    var pgPort = pgConfig["Port"] ?? "5432";
    var pgDb = pgConfig["Database"] ?? "aletrail";
    var pgUser = pgConfig["User"] ?? "aletrail_user";
    var pgPassword = pgConfig["Password"] ?? "aletrail_password";
    
    connectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword}";
}
// For production, read from environment variables or config
else
{
    var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? configuration["Postgres:Host"] ?? "postgres";
    var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? configuration["Postgres:Port"] ?? "5432";
    var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? configuration["Postgres:Database"] ?? "aletrail";
    var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? configuration["Postgres:User"] ?? "aletrail_user";
    var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? configuration["Postgres:Password"] ?? string.Empty;
    
    connectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword}";
}

// Register PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register IUserRepository implementation (now uses EF Core DbContext with fallback support)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPOIService, POIService>();
builder.Services.AddHttpClient("overpass", client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});
builder.Services.AddHostedService<BarSyncBackgroundService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the text input below.\n\nExample: \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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