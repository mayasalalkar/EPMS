using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Epms.Api.Data;
using Epms.Api.Middleware;
using Epms.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "EPMS API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"])
    .AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddDbContext<AppDbContext>((services, options) =>
{
    var config = services.GetRequiredService<IConfiguration>();
    var provider = config["DatabaseProvider"];
    if (provider == "Sqlite")
        options.UseSqlite(config.GetConnectionString("Sqlite")
            ?? throw new InvalidOperationException("Set ConnectionStrings:Sqlite."));
    else if (provider == "SqlServer")
        options.UseSqlServer(config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Set ConnectionStrings:SqlServer."));
    else
        throw new InvalidOperationException("DatabaseProvider must be SqlServer or Sqlite.");
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton(services =>
{
    var config = services.GetRequiredService<IConfiguration>();
    var env = services.GetRequiredService<IHostEnvironment>();
    var configuredKey = config["Jwt:Key"];
    var key = string.IsNullOrWhiteSpace(configuredKey)
        ? env.IsDevelopment() || env.IsEnvironment("Testing") ? RandomNumberGenerator.GetBytes(32) : []
        : Encoding.UTF8.GetBytes(configuredKey);
    if (key.Length < 32)
        throw new InvalidOperationException("Set Jwt:Key to a strong key of at least 32 bytes.");
    var expiryMinutes = config.GetValue<int>("Jwt:ExpiryMinutes");
    if (expiryMinutes is < 1 or > 1440)
        throw new InvalidOperationException("Jwt:ExpiryMinutes must be between 1 and 1440.");
    return new JwtSettings(new SymmetricSecurityKey(key),
        config["Jwt:Issuer"] ?? throw new InvalidOperationException("Set Jwt:Issuer."),
        config["Jwt:Audience"] ?? throw new InvalidOperationException("Set Jwt:Audience."), expiryMinutes);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<JwtSettings>((options, jwt) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.Key,
            ValidateLifetime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var user = context.Principal;
                if (!int.TryParse(user?.FindFirst("sub")?.Value, out var userId))
                {
                    context.Fail("Invalid account.");
                    return;
                }
                var database = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var account = await database.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId,
                    context.HttpContext.RequestAborted);
                if (account is null || user!.FindFirst("role")?.Value != account.Role.ToString() ||
                    user.FindFirst("employeeId")?.Value != account.EmployeeId?.ToString())
                    context.Fail("Invalid account.");
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
_ = app.Services.GetRequiredService<JwtSettings>();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        await DbSeeder.SeedAsync(db, app.Configuration["Demo:AdminPassword"],
            app.Configuration["Demo:EmployeePassword"], app.Environment.IsEnvironment("Testing"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
