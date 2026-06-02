using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class InterfacesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    public InterfacesController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public IActionResult solicitud_interfaces()
    {
        ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(ViewBag.Usuario)) return RedirectToAction("Index", "Home");
        
        return View("~/Views/Home/solicitud_interfaces.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GuardarInterfaces(Interfaces interfaces, List<IFormFile> evidenciaFiles)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");

        if (string.IsNullOrEmpty(usuarioSesion))
            return RedirectToAction("Index", "Home");

        // Validación de campos vacíos (mismo comportamiento que Altas y Bajas)
        if (
            string.IsNullOrWhiteSpace(interfaces.contacto) || string.IsNullOrWhiteSpace(interfaces.telefono) ||
            string.IsNullOrWhiteSpace(interfaces.extension) || string.IsNullOrWhiteSpace(interfaces.celular) ||
            string.IsNullOrWhiteSpace(interfaces.puesto) || string.IsNullOrWhiteSpace(interfaces.descripcion_proceso) ||
            evidenciaFiles == null || evidenciaFiles.Count == 0
        )
        {
            ViewBag.ErrorMessage = "Todos los campos son obligatorios, incluyendo la evidencia.";
            ViewBag.Usuario = usuarioSesion;

            return View("~/Views/Home/solicitud_interfaces.cshtml", interfaces);
        }

        try
        {
            // Procesamiento de archivos
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

            // Guardar los nombres de los archivos como string separado por comas
            interfaces.evidencia = string.Join(",", nombresArchivos);

            // Guardar en la tabla Interfaces
            _context.Interfaces.Add(interfaces);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Solicitud de interfaz registrada correctamente.";
            return RedirectToAction("solicitud_interfaces", "Interfaces");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error al guardar la solicitud: " + ex.Message;
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_interfaces.cshtml", interfaces);
        }
    }
}