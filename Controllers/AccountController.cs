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

            if (uOriginal != null && uOriginal.StartsWith("Gerencia ", StringComparison.OrdinalIgnoreCase))
            {
                var numberPart = uOriginal.Substring(9);
                if (int.TryParse(numberPart, out int gNum) && gNum is >= 1 and <= 10)
                {
                    TempData["ShowServiceSelection"] = true;
                    return RedirectToAction("Index", "Home");
                }
            }
            else if (uOriginal != null && uOriginal.StartsWith("Sistema ", StringComparison.OrdinalIgnoreCase))
            {
                var numberPart = uOriginal.Substring(8);
                if (int.TryParse(numberPart, out int sNum) && sNum is >= 1 and <= 5)
                {
                    return RedirectToAction("historial_servicio", "Historial");
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