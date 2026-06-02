using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class BP_VentasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public BP_VentasController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public IActionResult solicitud_bp_ventas()
    {
        ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(ViewBag.Usuario)) return RedirectToAction("Index", "Home");
        
        return View("~/Views/Home/solicitud_bp_ventas.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GuardarBPVentas(BusinessPro business_pro, List<IFormFile> evidenciaFiles)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");

        if (string.IsNullOrEmpty(usuarioSesion))
            return RedirectToAction("Index", "Home");

        // Validación de campos obligatorios
        if (
            string.IsNullOrWhiteSpace(business_pro.contacto) || string.IsNullOrWhiteSpace(business_pro.telefono) ||
            string.IsNullOrWhiteSpace(business_pro.extension) || string.IsNullOrWhiteSpace(business_pro.celular) ||
            string.IsNullOrWhiteSpace(business_pro.puesto) || string.IsNullOrWhiteSpace(business_pro.descripcion_proceso)
        )
        {
            ViewBag.ErrorMessage = "Todos los campos son obligatorios.";
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_bp_ventas.cshtml", business_pro);
        }

        try
        {
            // Procesamiento de archivos de evidencia
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
                business_pro.evidencia = string.Join(",", nombresArchivos);
            }

            business_pro.area = "Ventas";
            _context.BusinessPro.Add(business_pro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Solicitud de Business Pro del área de ventas ha sido registrada correctamente.";
            return RedirectToAction("solicitud_bp_ventas", "BP_Ventas");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error al guardar la solicitud: " + ex.Message;
            ViewBag.Usuario = usuarioSesion;
            return View("~/Views/Home/solicitud_bp_ventas.cshtml", business_pro);
        }
    }
}