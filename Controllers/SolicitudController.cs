using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class SolicitudController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public SolicitudController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private IActionResult CheckAuth(string viewName) {
        ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(ViewBag.Usuario)) return RedirectToAction("Index", "Home");
        return View($"~/Views/Home/{viewName}.cshtml");
    }

    public IActionResult solicitud_servicio() => CheckAuth("solicitud_servicio");
    
    [HttpPost]
    public async Task<IActionResult> GuardarSolicitud(OrdenServicio orden, List<IFormFile> evidenciaFiles)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuarioSesion)) return RedirectToAction("Index", "Home");

        if (string.IsNullOrWhiteSpace(orden.nomenclatura) || string.IsNullOrWhiteSpace(orden.descripcion) || evidenciaFiles == null || evidenciaFiles.Count == 0)
        {
            ViewBag.ErrorMessage = "Todos los campos son obligatorios, incluyendo la evidencia.";
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_servicio.cshtml", orden);
        }

        try
        {
            orden.nombre_cliente = usuarioSesion;

            // Si el número de serie es nulo (el usuario eligió "No"), asigna una cadena vacía.
            if (orden.numero_serie == null) {
                orden.numero_serie = "";
            }

            List<string> nombresArchivos = new List<string>();
            string rutaCarpeta = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

            foreach (var foto in evidenciaFiles)
            {
                string originalFileName = Path.GetFileName(foto.FileName); // Obtener solo el nombre del archivo, sin ruta
                string nombreUnico = Guid.NewGuid().ToString() + "_" + originalFileName;
                using (var stream = new FileStream(Path.Combine(rutaCarpeta, nombreUnico), FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }
                nombresArchivos.Add(nombreUnico);
            }

            orden.evidencia = string.Join(",", nombresArchivos);
            _context.OrdenesServicios.Add(orden);
            await _context.SaveChangesAsync();

            _context.DetalleOrdenServicios.Add(new DetalleOrdenServicio {
                id_orden = orden.id_orden,
                estatus = "nuevo",
                fecha_registro = DateTime.Now,
                comentarios = ""
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Ticket guardado. Folio: TICKET-{orden.id_orden:D3}";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error: " + ex.Message;
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_servicio.cshtml", orden);
        }
    }
}