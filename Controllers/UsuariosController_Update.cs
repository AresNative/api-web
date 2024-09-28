using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyApiProject.Models;

namespace MyApiProject.Controllers
{
    [Route("api/v1/users/update")]
    [ApiController]
    public class UsuariosController_Update : BaseController
    {
        public UsuariosController_Update(IConfiguration configuration) : base(configuration) { }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] Usuario usuarioActualizado)
        {
            string query = @"UPDATE Website_users 
                             SET Nombre = @Nombre, Email = @Email 
                             WHERE Id = @Id";

            try
            {
                await using var connection = await OpenConnection();
                await using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Nombre", usuarioActualizado.name);
                command.Parameters.AddWithValue("@Email", usuarioActualizado.email);
                command.Parameters.AddWithValue("@Id", id);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    return Ok(new { Message = "Usuario actualizado exitosamente" });
                else
                    return NotFound(new { Message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

}
