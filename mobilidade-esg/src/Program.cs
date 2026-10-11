using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MobilidadeESG.Data;
using MobilidadeESG.Middlewares;
using MobilidadeESG.Repositories;
using MobilidadeESG.Repositories.Interfaces;
using MobilidadeESG.Services;
using MobilidadeESG.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Banco de Dados ──────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)
    )
);

// ── Repositórios ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IVeiculoEletricoRepository, VeiculoEletricoRepository>();
builder.Services.AddScoped<IRotaTransporteRepository, RotaTransporteRepository>();
builder.Services.AddScoped<IRegistroUsoRepository, RegistroUsoRepository>();
builder.Services.AddScoped<IRecompensaRepository, RecompensaRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// ── Services ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IVeiculoEletricoService, VeiculoEletricoService>();
builder.Services.AddScoped<ITransportePublicoService, TransportePublicoService>();
builder.Services.AddScoped<IRecompensaService, RecompensaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── JWT Auth ────────────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ── Controllers + Validação ──────────────────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true; // Tratamos manualmente nos controllers
    });

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MobilidadeESG API",
        Version = "v1",
        Description = "API para monitoramento e incentivo de mobilidade sustentável - Tema ESG FIAP",
        Contact = new OpenApiContact { Name = "Grupo FIAP", Email = "grupo@fiap.com.br" }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Insira: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Pipeline ──────────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

// Swagger: ligado em Development/Staging; em Production só se Swagger__Enabled=true
var swaggerEnabled = !app.Environment.IsProduction()
    || app.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MobilidadeESG API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

// Dentro do container o TLS é responsabilidade do proxy/host; fora dele, redireciona para HTTPS
if (!app.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Health check (usado pelo Docker, pelo pipeline e pelos smoke tests) ──────
app.MapGet("/health", async (AppDbContext db, IWebHostEnvironment env, IConfiguration config) =>
{
    var dbOk = await db.Database.CanConnectAsync();
    var body = new
    {
        status = dbOk ? "Healthy" : "Unhealthy",
        environment = env.EnvironmentName,
        version = config["APP_VERSION"] ?? "local",
        database = dbOk ? "Connected" : "Unavailable",
        timestampUtc = DateTime.UtcNow
    };
    return dbOk ? Results.Ok(body) : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
})
.AllowAnonymous()
.WithTags("Health");

// ── Banco: aplica migrations na subida (todos os ambientes) ──────────────────
// Database__AutoMigrate=false desliga esse comportamento.
if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (db.Database.IsRelational() && db.Database.GetMigrations().Any())
    {
        logger.LogInformation("Aplicando migrations no ambiente {Env}...", app.Environment.EnvironmentName);
        db.Database.Migrate();
    }
    else
    {
        // InMemory (testes) ou projeto sem pasta Migrations: cria o schema a partir do modelo
        logger.LogWarning("Nenhuma migration encontrada (ou provider não relacional). Usando EnsureCreated().");
        db.Database.EnsureCreated();
    }
}

app.Run();

// Classe parcial necessária para os testes de integração
public partial class Program { }
