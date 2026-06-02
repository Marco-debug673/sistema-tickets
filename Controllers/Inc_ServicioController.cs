using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class Inc_ServicioController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public Inc_ServicioController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public IActionResult solicitud_incadea_servicio()
    {
        ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(ViewBag.Usuario)) return RedirectToAction("Index", "Home");
        
        return View("~/Views/Home/solicitud_incadea_servicio.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GuardarIncServicio(Incadea incadea, List<IFormFile> evidenciaFiles)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");

        if (string.IsNullOrEmpty(usuarioSesion))
            return RedirectToAction("Index", "Home");

        // Validación de campos obligatorios
        if (
            string.IsNullOrWhiteSpace(incadea.contacto) || string.IsNullOrWhiteSpace(incadea.telefono) ||
            string.IsNullOrWhiteSpace(incadea.extension) || string.IsNullOrWhiteSpace(incadea.celular) ||
            string.IsNullOrWhiteSpace(incadea.puesto) || string.IsNullOrWhiteSpace(incadea.descripcion_proceso)
        )
        {
            ViewBag.ErrorMessage = "Todos los campos son obligatorios.";
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_incadea_servicio.cshtml", incadea);
        }

        try
        {
            // Procesamiento de archivos de evidencia
            incadea.evidencia = ""; // Inicializar para cumplir con el modelo
            if (evidenciaFiles != null && evidenciaFiles.Count > 0)
            {
                List<string> nombresArchivos = new List<string>();
                string rutaCarpeta = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

                foreach (var file in evidenciaFiles)
                {
                    string nombreUnico = Guid.NewGuid().ToString() + "_" + file.FileName;
                    using (var stream = new FileStream(Path.Combine(rutaCarpeta, nombreUnico), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    nombresArchivos.Add(nombreUnico);
                }
                incadea.evidencia = string.Join(",", nombresArchivos);
            }

            // Asignación del área específica
            incadea.area = "Servicio";
            _context.Incadea.Add(incadea);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Solicitud de Incadea del área de servicio ha sido registrada correctamente.";
            return RedirectToAction("solicitud_incadea_servicio", "Inc_Servicio");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error al guardar la solicitud: " + ex.Message;
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_incadea_servicio.cshtml", incadea);
        }
    }
}