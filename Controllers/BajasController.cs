using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class BajasController : Controller
{
    private readonly AppDbContext _context;
    public BajasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult solicitud_bajas()
    {
        ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(ViewBag.Usuario)) return RedirectToAction("Index", "Home");
        return View("~/Views/Home/solicitud_bajas.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GuardarBajas(Bajas bajas)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");

        if (string.IsNullOrEmpty(usuarioSesion))
            return RedirectToAction("Index", "Home");

        // Validación de campos vacíos (mismos que Altas)
        if (
            string.IsNullOrWhiteSpace(bajas.nombre) || string.IsNullOrWhiteSpace(bajas.nomenclatura) ||
            string.IsNullOrWhiteSpace(bajas.empresa) || string.IsNullOrWhiteSpace(bajas.sucursal) ||
            string.IsNullOrWhiteSpace(bajas.puesto) || string.IsNullOrWhiteSpace(bajas.departamento) ||
            string.IsNullOrWhiteSpace(bajas.descripcion)
        )
        {
            ViewBag.ErrorMessage = "Todos los campos son obligatorios.";
            ViewBag.Usuario = usuarioSesion;

            return View("~/Views/Home/solicitud_bajas.cshtml", bajas);
        }

        try
        {
            // Guardar en tabla bajas
            _context.Bajas.Add(bajas);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Baja registrada correctamente.";
            return RedirectToAction("solicitud_bajas", "Bajas");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error al guardar la solicitud: " + ex.Message;
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_bajas.cshtml", bajas);
        }
    }
}