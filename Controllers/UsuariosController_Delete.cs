using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;


namespace MyApiProject.Controllers
{
    [Route("api/v1/users/delete_user")]
    [ApiController]
    public class UsuariosController_Delete : BaseController
    {
        public UsuariosController_Delete(IConfiguration configuration) : base(configuration) { }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            string query = @"DELETE FROM Website_users WHERE Id = @Id";

            try
            {
                await using var connection = await OpenConnection();
                await using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", id);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                    return Ok(new { Message = "Usuario eliminado exitosamente" });
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
