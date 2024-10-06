// LogUtils.cs
using Microsoft.IdentityModel.Tokens;
using MyApiProject.Models;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging; // Para el manejo de logs

public class LogUtils
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LogUtils> _logger;
    DateTime now = DateTime.Now;

    // Constructor para inyectar IConfiguration y ILogger
    public LogUtils(IConfiguration configuration, ILogger<LogUtils> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> IsValidUser(LoginModel login)
    {
        string query = "SELECT COUNT(1) FROM Website_users WHERE email = @Email AND password = @Password";
        try
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);

            // Asegúrate de usar hashes para las contraseñas en un entorno real
            command.Parameters.AddWithValue("@Email", login.Email);
            command.Parameters.AddWithValue("@Password", login.Password); // Hash recomendado

            var result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el usuario.");
            return false;
        }
    }

    public async Task<int> IsValidUserId(int id)
    {
        string query = "SELECT id FROM Website_users WHERE id = @ID";
        try
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);

            // Asegúrate de usar hashes para las contraseñas en un entorno real
            command.Parameters.AddWithValue("@ID", id);

            //var result = await command.ExecuteScalarAsync();
            var result = (int)await command.ExecuteScalarAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el usuario.");
            return -1;
        }
    }

    public async Task<int> GetUserId(string email)
    {
        string query = "SELECT id FROM Website_users WHERE email = @Email";
        try
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            var result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0; // Verifica si el resultado es null
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el ID del usuario.");
            throw new Exception("Error al obtener el ID del usuario.");
        }
    }

    public async Task InsertUserHistory(int userId, string movement)
    {
        string query = "INSERT INTO Website_user_history (id_user, mov, date) VALUES (@IdUser, @Movement, @Timestamp)";
        try
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdUser", userId);
            command.Parameters.AddWithValue("@Movement", movement);
            command.Parameters.AddWithValue("@Timestamp", now);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar el historial del usuario.");
            throw new Exception("Error al insertar el historial del usuario.");
        }
    }

    public async Task InsertUserSession(int userId, string token, int active)
    {
        string query = "INSERT INTO Website_sessions (id_user, token, active) VALUES (@IdUser, @Token, @Active)";
        try
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdUser", userId);
            command.Parameters.AddWithValue("@Token", token);
            command.Parameters.AddWithValue("@Active", active);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar el historial del usuario.");
            throw new Exception("Error al insertar el historial del usuario.");
        }
    }

    public string GenerateJwtToken(LoginModel login)
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, login.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, login.Email)
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
