using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;
using System.Linq;

namespace SistemaTickets.Controllers
{
    public class CrearCuentasController : Controller
    {
        private readonly AppDbContext _context;

        public CrearCuentasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /CrearCuentas
        [HttpGet]
        public IActionResult Index()
        {
            // Simplemente muestra el formulario de creación.
            return View("~/Views/Home/CrearCuentas.cshtml");
        }

        // POST: /CrearCuentas/Crear
        [HttpPost]
        [ActionName("Crear")]
        public IActionResult Crear(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "El nombre de usuario y la contraseña son obligatorios.";
                return RedirectToAction("Index");
            }

            var cleanUsername = username.Trim();

            if (_context.Usuarios.Any(u => u.nombre_usuario != null && u.nombre_usuario.Trim() == cleanUsername))
            {
                TempData["ErrorMessage"] = $"El nombre de usuario '{cleanUsername}' ya existe. Por favor, elige otro.";
                return RedirectToAction("Index");
            }

            var nuevoUsuario = new Usuario { nombre_usuario = cleanUsername, contrasena = password.Trim() };
            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"¡La cuenta '{cleanUsername}' ha sido creada éxitosamente!";
            return RedirectToAction("Index");
        }
    }
}