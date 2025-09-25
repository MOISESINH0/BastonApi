using Baston.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Servicios ---
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Controladores
builder.Services.AddControllers();

// --- CORS (permite llamadas desde Flutter/emulador) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// --- JWT ---
var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = string.Empty;
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Baston.Api v1");
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFlutter");   // 👈 importante para Flutter
app.UseAuthentication();       // 👈 activa autenticación JWT
app.UseAuthorization();

app.MapControllers();

// --- Endpoints extra de BD (opcionales para diagnóstico) ---
// Salud de la BD
app.MapGet("/db/health", async (AppDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return Results.Ok(new { database = ok ? "up" : "down", timeUtc = DateTime.UtcNow });
});

// Listar tablas del esquema public
app.MapGet("/db/tables", async (AppDbContext db) =>
{
    await using var conn = db.Database.GetDbConnection();
    await conn.OpenAsync();

    await using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema='public'
        ORDER BY table_name
    """;

    var tables = new List<string>();
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        tables.Add(reader.GetString(0));

    return Results.Ok(tables);
});

// Listar columnas de una tabla
app.MapGet("/db/tables/{name}", async (string name, AppDbContext db) =>
{
    await using var conn = db.Database.GetDbConnection();
    await conn.OpenAsync();

    await using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT column_name, data_type, is_nullable
        FROM information_schema.columns
        WHERE table_schema='public' AND table_name = @name
        ORDER BY ordinal_position
    """;
    var p = cmd.CreateParameter();
    p.ParameterName = "@name";
    p.Value = name;
    cmd.Parameters.Add(p);

    var cols = new List<object>();
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        cols.Add(new
        {
            column = reader.GetString(0),
            type = reader.GetString(1),
            nullable = reader.GetString(2) == "YES"
        });

    return Results.Ok(cols);
});

// Ver migraciones aplicadas
app.MapGet("/db/migrations", async (AppDbContext db) =>
{
    var applied = await db.Database.GetAppliedMigrationsAsync();
    return Results.Ok(applied);
});

// --- Migraciones automáticas al iniciar ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
