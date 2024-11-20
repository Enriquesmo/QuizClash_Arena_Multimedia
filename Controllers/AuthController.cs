using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuizClash_Arena_Multimedia.Controllers
{
    public class AuthController : Controller
    {
        // Ruta para redirigir a Twitch para autenticarse
        [HttpGet("auth/login")]
        public IActionResult Login()
        {
            // Redirige al usuario a Twitch para que se autentique
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Twitch");
        }

        // Ruta para recibir el código de autorización de Twitch
        [HttpGet("auth/callback")]
        public async Task<IActionResult> Callback()
        {
            // Obtener el resultado de la autenticación desde el contexto
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return BadRequest("Error de autenticación");  // Si la autenticación falla
            }

            // Extraer los datos del usuario autenticado
            var user = authenticateResult.Principal;

            // Obtener el nombre y el correo del usuario desde los claims (si están disponibles)
            var username = user?.FindFirstValue(ClaimTypes.Name);
            var email = user?.FindFirstValue(ClaimTypes.Email);

            // Opcionalmente, puedes hacer algo más con los datos del usuario, como guardarlos en una base de datos o una cookie

            return Ok(new
            {
                Username = username,
                Email = email,
                Message = "Autenticación exitosa"
            });
        }

        // Ruta para cerrar la sesión
        [HttpGet("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            // Cerrar la sesión
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");  // Redirigir al inicio
        }
    }
}
