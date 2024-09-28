using Microsoft.AspNetCore.Mvc;
using MyApiProject.Models;
namespace MyApiProject.Controllers
{
    [Route("api/v1/users/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LogUtils _logUtils;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, LogUtils logUtils)
        {
            _configuration = configuration;
            _logUtils = logUtils;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            // Aquí validarías las credenciales contra la base de datos
            if (await _logUtils.IsValidUser(login))
            {
                var token = _logUtils.GenerateJwtToken(login);
                return Ok(new { Token = token });
            }
            else
            {
                return Unauthorized(new { Message = "Credenciales inválidas" });
            }
        }
    }
}
