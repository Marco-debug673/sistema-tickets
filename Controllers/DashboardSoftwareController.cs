using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class DashboardSoftwareController : Controller
{
    private readonly AppDbContext _context;

    public DashboardSoftwareController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        ViewBag.Usuario = usuarioSesion;
        if (string.IsNullOrEmpty(usuarioSesion)) return RedirectToAction("Index", "Home");

        var softwareItems = await GetSoftwareItemsInternalAsync();

        ViewBag.Total = softwareItems.Count;
        ViewBag.Nuevo = softwareItems.Count(t => t.Estatus == "nuevo");
        ViewBag.Proceso = softwareItems.Count(t => t.Estatus == "proceso");
        ViewBag.Cerrado = softwareItems.Count(t => t.Estatus == "cerrado");

        return View("~/Views/Home/dashboard_software.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData()
    {
        var softwareItems = await GetSoftwareItemsInternalAsync();
        return Json(new
        {
            total = softwareItems.Count,
            nuevo = softwareItems.Count(t => t.Estatus == "nuevo"),
            proceso = softwareItems.Count(t => t.Estatus == "proceso"),
            cerrado = softwareItems.Count(t => t.Estatus == "cerrado")
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketsByStatus(string status)
    {
        if (string.IsNullOrEmpty(status)) return BadRequest("El estatus es requerido.");

        var softwareItems = await GetSoftwareItemsInternalAsync();
        var filteredTickets = softwareItems
            .Where(t => t.Estatus == status)
            .OrderByDescending(t => t.Id)
            .ToList();

        return Json(filteredTickets);
    }

    // Este método es una copia del que se encuentra en HistorialSoftwareController
    // para mantener la lógica de obtención de datos centralizada aquí.
    private async Task<List<HistorialSoftwareViewModel>> GetSoftwareItemsInternalAsync()
    {
        var softwareItems = new List<HistorialSoftwareViewModel>();
    
        // 1. Altas
        var altas = await (from a in _context.Altas
                           join d in _context.DetalleSoftwareServicios on a.id_alta equals d.id_alta into details
                           from latestDetail in details.OrderByDescending(d => d.Id).Take(1).DefaultIfEmpty()
                           select new HistorialSoftwareViewModel {
                               Id = a.id_alta, TipoSolicitud = "Altas", Nombre = a.nombre, Nomenclatura = a.nomenclatura,
                               Empresa = a.empresa, Sucursal = a.sucursal, Puesto = a.puesto, Departamento = a.departamento,
                               Descripcion = a.descripcion, OrigenTabla = "Altas",
                               Estatus = latestDetail != null ? latestDetail.Estatus : "nuevo",
                               AsignadoA = latestDetail != null ? latestDetail.AsignadoA : "",
                               Comentarios = latestDetail != null ? latestDetail.Comentarios : ""
                           }).ToListAsync();
        softwareItems.AddRange(altas);
    
        // 2. Bajas
        var bajas = await (from b in _context.Bajas
                           join d in _context.DetalleSoftwareServicios on b.id_baja equals d.id_baja into details
                           from latestDetail in details.OrderByDescending(d => d.Id).Take(1).DefaultIfEmpty()
                           select new HistorialSoftwareViewModel {
                               Id = b.id_baja, TipoSolicitud = "Bajas", Nombre = b.nombre, Nomenclatura = b.nomenclatura,
                               Empresa = b.empresa, Sucursal = b.sucursal, Puesto = b.puesto, Departamento = b.departamento,
                               Descripcion = b.descripcion, OrigenTabla = "Bajas",
                               Estatus = latestDetail != null ? latestDetail.Estatus : "nuevo",
                               AsignadoA = latestDetail != null ? latestDetail.AsignadoA : "",
                               Comentarios = latestDetail != null ? latestDetail.Comentarios : ""
                           }).ToListAsync();
        softwareItems.AddRange(bajas);
    
        // 3. Business Pro
        var bp = await (from b in _context.BusinessPro
                        join d in _context.DetalleSoftwareServicios on b.id_bp equals d.id_bp into details
                        from latestDetail in details.OrderByDescending(d => d.Id).Take(1).DefaultIfEmpty()
                        select new HistorialSoftwareViewModel {
                            Id = b.id_bp, TipoSolicitud = "Business Pro", Area = b.area, Contacto = b.contacto,
                            Telefono = b.telefono, Extension = b.extension, Celular = b.celular, 
                            Evidencia = b.evidencia, Puesto = b.puesto, Descripcion = b.descripcion_proceso,
                            OrigenTabla = "BusinessPro",
                            Estatus = latestDetail != null ? latestDetail.Estatus : "nuevo",
                            AsignadoA = latestDetail != null ? latestDetail.AsignadoA : "",
                            Comentarios = latestDetail != null ? latestDetail.Comentarios : ""
                        }).ToListAsync();
        softwareItems.AddRange(bp);
    
        // 4. Interfaces
        var inter = await (from i in _context.Interfaces
                           join d in _context.DetalleSoftwareServicios on i.id_interfaces equals d.id_interfaces into details
                           from latestDetail in details.OrderByDescending(d => d.Id).Take(1).DefaultIfEmpty()
                           select new HistorialSoftwareViewModel {
                               Id = i.id_interfaces, TipoSolicitud = "Interfaces", Contacto = i.contacto,
                               Telefono = i.telefono, Extension = i.extension, Celular = i.celular,
                               Evidencia = i.evidencia, Puesto = i.puesto, Descripcion = i.descripcion_proceso,
                               OrigenTabla = "Interfaces",
                               Estatus = latestDetail != null ? latestDetail.Estatus : "nuevo",
                               AsignadoA = latestDetail != null ? latestDetail.AsignadoA : "",
                               Comentarios = latestDetail != null ? latestDetail.Comentarios : ""
                           }).ToListAsync();
        softwareItems.AddRange(inter);
    
        // 5. Incadea
        var inc = await (from i in _context.Incadea
                         join d in _context.DetalleSoftwareServicios on i.id_incadea equals d.id_incadea into details
                         from latestDetail in details.OrderByDescending(d => d.Id).Take(1).DefaultIfEmpty()
                         select new HistorialSoftwareViewModel {
                             Id = i.id_incadea, TipoSolicitud = "Incadea", Area = i.area, Contacto = i.contacto,
                             Telefono = i.telefono, Extension = i.extension, Celular = i.celular,
                             Evidencia = i.evidencia, Puesto = i.puesto, Descripcion = i.descripcion_proceso,
                             OrigenTabla = "Incadea",
                             Estatus = latestDetail != null ? latestDetail.Estatus : "nuevo",
                             AsignadoA = latestDetail != null ? latestDetail.AsignadoA : "",
                             Comentarios = latestDetail != null ? latestDetail.Comentarios : ""
                         }).ToListAsync();
        softwareItems.AddRange(inc);
    
        return softwareItems;
    }
}