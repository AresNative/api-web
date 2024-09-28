// LogUtils.cs
using Microsoft.IdentityModel.Tokens;
using MyApiProject.Models;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class LogUtils
{
    private readonly IConfiguration _configuration;

    // Constructor para inyectar IConfiguration
    public LogUtils(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> IsValidUser(LoginModel login)
    {
        // Lógica para validar si el usuario y contraseña son correctos
        // Puedes hacer la consulta a la base de datos para validar el usuario
        string query = "SELECT COUNT(1) FROM Website_users WHERE email = @Email AND password = @Password";
        try
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", login.Email);
            command.Parameters.AddWithValue("@Password", login.Password); // Asegúrate de usar hashes en un entorno real

            var result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            // Manejar la excepción
            Console.WriteLine(ex);
            return false;
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
            command.Parameters.AddWithValue("@IdUser", email);

            var result = await command.ExecuteScalarAsync();
            return (int)result;
        }
        catch (Exception ex)
        {
            // Manejar la excepción
            Console.WriteLine(ex);
            throw new Exception("Error al obtener el ID del usuario.");
        }
    }
    public async Task InsertUserHistory(int userId, string movement)
    {
        string query = "INSERT INTO Website_user_history (id_user, mov) VALUES (@IdUser, @Movement)";
        try
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdUser", userId);
            command.Parameters.AddWithValue("@Movement", movement);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Manejar la excepción
            Console.WriteLine(ex);
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