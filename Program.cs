
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<LogUtils>();
// Leer los orígenes permitidos desde appsettings.json
var allowedCorsOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>();

// Agregar los servicios a la colección de servicios (como controladores)
builder.Services.AddControllers().AddNewtonsoftJson();

// Definir la política de CORS desde configuración
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(allowedCorsOrigins)  // Cargar orígenes desde appsettings.json
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Agregar autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
        };
    });

// Agregar el servicio de Swagger para documentación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Crear instancia de Startup y pasarle la configuración
var startup = new Startup(builder.Configuration);

// Llamar a ConfigureServices del Startup
startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Acceso a Swagger en la raíz
});

app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();
app.UseAuthentication();  // Asegurarse de usar autenticación
app.UseAuthorization();
app.MapControllers();

app.Run();
