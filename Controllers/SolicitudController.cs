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
    public async Task<IActionResult> GuardarSolicitud(OrdenServicio orden, List<IFormFile> fotoServicio)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(orden.nomenclatura) || string.IsNullOrEmpty(orden.descripcion) || fotoServicio == null || fotoServicio.Count == 0)
        {
            ViewBag.ErrorMessage = "Campos obligatorios faltantes.";
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_servicio.cshtml", orden);
        }

        try
        {
            orden.nombre_cliente = usuarioSesion ?? "Desconocido";
            List<string> nombresArchivos = new List<string>();
            string rutaCarpeta = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

            foreach (var foto in fotoServicio)
            {
                string nombreUnico = Guid.NewGuid().ToString() + "_" + foto.FileName;
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
            return View("~/Views/Home/solicitud_servicio.cshtml", orden);
        }
    }
}