using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class AltasController : Controller
{
    private readonly AppDbContext _context;

    public AltasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult solicitud_altas()
    {
        ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(ViewBag.Usuario)) return RedirectToAction("Index", "Home");
        return View("~/Views/Home/solicitud_altas.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GuardarAltas(Altas alta)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");

        if (string.IsNullOrEmpty(usuarioSesion))
        return RedirectToAction("Index", "Home");

        // Validación de campos vacíos
        if (
            string.IsNullOrWhiteSpace(alta.nombre) || string.IsNullOrWhiteSpace(alta.nomenclatura) ||
            string.IsNullOrWhiteSpace(alta.empresa) || string.IsNullOrWhiteSpace(alta.sucursal) ||
            string.IsNullOrWhiteSpace(alta.puesto) || string.IsNullOrWhiteSpace(alta.departamento) ||
            string.IsNullOrWhiteSpace(alta.descripcion)
        )
        {
            ViewBag.ErrorMessage = "Todos los campos son obligatorios.";
            ViewBag.Usuario = usuarioSesion;

            return View("~/Views/Home/solicitud_altas.cshtml", alta);
        }

        try
        {
            // Guardar en tabla altas
            _context.Altas.Add(alta);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Alta registrada correctamente.";
            return RedirectToAction("solicitud_altas", "Altas");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error al guardar la solicitud: " + ex.Message;

            return View("~/Views/Home/solicitud_altas.cshtml", alta);
        }
    }
}