using Microsoft.AspNetCore.Mvc;
using MyApiProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace MyApiProject.Controllers
{
    [Route("api/v1/users/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthUtils _authUtils;

        public AuthController(AuthUtils authUtils)
        {
            _authUtils = authUtils;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            // Aquí validarías las credenciales contra la base de datos
            if (await _authUtils.IsValidUser(login))
            {
                var token = _authUtils.GenerateJwtToken(login);
                var userId = await _authUtils.GetUserId(login.Email);
                await _authUtils.InsertUserSession(userId, token, '1');
                await _authUtils.InsertUserHistory(userId, "LogIn");
                return Ok(new { Token = token, UserId = userId });
            }
            else
            {
                return Unauthorized(new { Message = "Credenciales inválidas" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> logOut(int id)
        {
            // Aquí validarías las credenciales contra la base de datos
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = _authUtils.GetUserIdFromToken(token);

            await _authUtils.InsertUserHistory(userId, "LogOut");
            await _authUtils.Logout(userId, token);
            return Ok(new { Message = "Logout exitoso." });
        }
    }
}
