using Microsoft.AspNetCore.Mvc;
using MyApiProject.Models;

namespace MyApiProject.Controllers
{
    [Route("api/v1/users/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LogUtils _logUtils;

        public AuthController(LogUtils logUtils)
        {
            _logUtils = logUtils;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            // Aquí validarías las credenciales contra la base de datos
            if (await _logUtils.IsValidUser(login))
            {
                var token = _logUtils.GenerateJwtToken(login);
                var userId = await _logUtils.GetUserId(login.Email);
                await _logUtils.InsertUserSession(userId, token, '1');
                await _logUtils.InsertUserHistory(userId, "LogIn");
                return Ok(new { Token = token, UserId = userId });
            }
            else
            {
                return Unauthorized(new { Message = "Credenciales inválidas" });
            }
        }
    }
}
