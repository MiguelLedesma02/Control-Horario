using System.Text;
using ControlHorario.Application.Interfaces;
using ControlHorario.Application.Services;
using ControlHorario.Infrastructure.Data;
using ControlHorario.Infrastructure.Export;
using ControlHorario.Infrastructure.Repositories;
using ControlHorario.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Servicios ───
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// EF Core
var provider = builder.Configuration["DatabaseProvider"];
var connStr = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    if (provider == "SqlServer") opts.UseSqlServer(connStr);
    else opts.UseSqlite(connStr);
});

// Repos
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();
builder.Services.AddScoped<IFichadaRepository, FichadaRepository>();
builder.Services.AddScoped<INovedadRepository, NovedadRepository>();
builder.Services.AddScoped<ICierreRepository, CierreRepository>();
builder.Services.AddScoped<IParametrosRepository, ParametrosRepository>();
builder.Services.AddScoped<IFeriadoRepository, FeriadoRepository>();
builder.Services.AddScoped<IExportadorService, ExportadorService>();

// Servicios
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ResumenService>();
builder.Services.AddScoped<CierreService>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IJwtService>(sp =>
{
    var c = sp.GetRequiredService<IConfiguration>();
    return new JwtService(
        c["Jwt:Key"]!, c["Jwt:Issuer"]!, c["Jwt:Audience"]!,
        int.Parse(c["Jwt:ExpiresHours"]!));
});

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        var c = builder.Configuration;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = c["Jwt:Issuer"],
            ValidAudience = c["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(c["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

// CORS
var origenes = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origenes).AllowAnyMethod().AllowAnyHeader()));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Control Horario API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT (sin el prefijo Bearer)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
          Array.Empty<string>() }
    });
});

var app = builder.Build();

// ─── Pipeline ───
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Migrations + Seed
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.EnsureCreated();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(ctx, hasher);
}

app.Run();
