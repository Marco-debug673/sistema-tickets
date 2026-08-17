using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AccountController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.ErrorMessage = "El usuario y la contraseña son obligatorios.";
            return View("~/Views/Home/Index.cshtml");
        }

        string cleanUsername = username.Trim();
        var usuario = _context.Usuarios.FirstOrDefault(u => u.nombre_usuario != null && u.nombre_usuario.Trim() == cleanUsername);

        bool esValido = false;
        if (usuario != null && !string.IsNullOrEmpty(usuario.contrasena))
            esValido = (password.Trim() == usuario.contrasena.Trim());

        if (esValido)
        {
            // Redirección específica para el usuario "Sistema 01" después de la validación
            if (usuario!.nombre_usuario?.Trim().Equals("Sistema 01", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("Index", "CrearCuentas");
            }
            HttpContext.Session.SetString("Usuario", usuario!.nombre_usuario ?? string.Empty);
            string? uname = usuario.nombre_usuario?.ToLower();
            string? uOriginal = usuario.nombre_usuario;

            if (uname == "gerencia001")
            {
                TempData["ShowHistorySelection"] = true;
                return RedirectToAction("Index", "Home");
            }

            if (uname is "asistente0011" or "asistente0012" or "jefe0018")
                return RedirectToAction("historial_servicio", "Historial");

            if (uname is "asistente0014" or "asistente0013" or "jefe0017")
                return RedirectToAction("Dashboard_software", "Dashboard");

            // Lógica para usuarios tipo "Gerencia" (gerencia, analista, etc.)
            if (uOriginal != null)
            {
                string[] gerenciaLikePrefixes = { "gerencia", "analista", "auxiliar", "supervisor", "asistente" };
                var lowerUOriginal = uOriginal.ToLower();
                foreach (var prefix in gerenciaLikePrefixes)
                {
                    if (lowerUOriginal.StartsWith(prefix))
                    {
                        var numberPart = uOriginal.Substring(prefix.Length);
                        if (int.TryParse(numberPart, out _))
                        {
                            TempData["ShowServiceSelection"] = true;
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            TempData["ShowServiceSelection"] = true;
            return RedirectToAction("Index", "Home");
        }

        ViewBag.ErrorMessage = "Usuario o contraseña incorrectos.";
        return View("~/Views/Home/Index.cshtml");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}