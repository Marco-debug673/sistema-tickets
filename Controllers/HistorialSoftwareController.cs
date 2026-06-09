using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class HistorialSoftwareController : Controller
{
    private readonly AppDbContext _context;

    public HistorialSoftwareController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(ViewBag.Usuario)) return RedirectToAction("Index", "Home");

        // Obtener sucursales únicas de Altas y Bajas para el filtro
        var sucursalesAltas = await _context.Altas.Select(a => a.sucursal).Distinct().ToListAsync();
        var sucursalesBajas = await _context.Bajas.Select(b => b.sucursal).Distinct().ToListAsync();
        ViewBag.Sucursales = sucursalesAltas.Concat(sucursalesBajas)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct().OrderBy(s => s).ToList();

        // Obtener empresas únicas de Altas y Bajas para el filtro
        var empresasAltas = await _context.Altas.Select(a => a.empresa).Distinct().ToListAsync();
        var empresasBajas = await _context.Bajas.Select(b => b.empresa).Distinct().ToListAsync();
        ViewBag.Empresas = empresasAltas.Concat(empresasBajas)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct().OrderBy(e => e).ToList();

        // Obtener nombres únicos (Altas, Bajas)
        var nombresAltas = await _context.Altas.Select(a => a.nombre).Distinct().ToListAsync();
        var nombresBajas = await _context.Bajas.Select(b => b.nombre).Distinct().ToListAsync();
        ViewBag.Nombres = nombresAltas.Concat(nombresBajas).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();

        // Obtener contactos únicos (BP, Interfaces, Incadea)
        var contactosBP = await _context.BusinessPro.Select(b => b.contacto).Distinct().ToListAsync();
        var contactosInter = await _context.Interfaces.Select(i => i.contacto).Distinct().ToListAsync();
        var contactosInc = await _context.Incadea.Select(i => i.contacto).Distinct().ToListAsync();
        ViewBag.Contactos = contactosBP.Concat(contactosInter).Concat(contactosInc).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();

        // Obtener nomenclaturas únicas (Altas, Bajas)
        var nomenAltas = await _context.Altas.Select(a => a.nomenclatura).Distinct().ToListAsync();
        var nomenBajas = await _context.Bajas.Select(b => b.nomenclatura).Distinct().ToListAsync();
        ViewBag.Nomenclaturas = nomenAltas.Concat(nomenBajas).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();

        // Obtener puestos únicos (Todas)
        var puestosAltas = await _context.Altas.Select(a => a.puesto).Distinct().ToListAsync();
        var puestosBajas = await _context.Bajas.Select(b => b.puesto).Distinct().ToListAsync();
        var puestosBP = await _context.BusinessPro.Select(b => b.puesto).Distinct().ToListAsync();
        var puestosInter = await _context.Interfaces.Select(i => i.puesto).Distinct().ToListAsync();
        var puestosInc = await _context.Incadea.Select(i => i.puesto).Distinct().ToListAsync();
        ViewBag.Puestos = puestosAltas.Concat(puestosBajas).Concat(puestosBP).Concat(puestosInter).Concat(puestosInc).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().OrderBy(p => p).ToList();

        // Obtener departamentos únicos (Altas, Bajas)
        var deptosAltas = await _context.Altas.Select(a => a.departamento).Distinct().ToListAsync();
        var deptosBajas = await _context.Bajas.Select(b => b.departamento).Distinct().ToListAsync();
        ViewBag.Departamentos = deptosAltas.Concat(deptosBajas).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().OrderBy(d => d).ToList();

        var softwareItems = await GetSoftwareItemsInternalAsync();

        return View("~/Views/Home/historial_servicio_software.cshtml", softwareItems.OrderByDescending(x => x.Id));
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketsByNomenclatura(string nomenclatura)
    {
        if (string.IsNullOrWhiteSpace(nomenclatura)) return Json(new List<HistorialSoftwareViewModel>());

        var allItems = await GetSoftwareItemsInternalAsync();
        var filtered = allItems.Where(x => x.Nomenclatura == nomenclatura).ToList();
        return Json(filtered);
    }

    private async Task<List<HistorialSoftwareViewModel>> GetSoftwareItemsInternalAsync()
    {
        var softwareItems = new List<HistorialSoftwareViewModel>();

        // 1. Altas
        var altas = await _context.Altas.Select(a => new HistorialSoftwareViewModel {
            Id = a.id_alta, TipoSolicitud = "Altas", Nombre = a.nombre, Nomenclatura = a.nomenclatura,
            Empresa = a.empresa, Sucursal = a.sucursal, Puesto = a.puesto, Departamento = a.departamento,
            Descripcion = a.descripcion, OrigenTabla = "Altas"
        }).ToListAsync();
        softwareItems.AddRange(altas);

        // 2. Bajas
        var bajas = await _context.Bajas.Select(b => new HistorialSoftwareViewModel {
            Id = b.id_baja, TipoSolicitud = "Bajas", Nombre = b.nombre, Nomenclatura = b.nomenclatura,
            Empresa = b.empresa, Sucursal = b.sucursal, Puesto = b.puesto, Departamento = b.departamento,
            Descripcion = b.descripcion, OrigenTabla = "Bajas"
        }).ToListAsync();
        softwareItems.AddRange(bajas);

        // 3. Business Pro
        var bp = await _context.BusinessPro.Select(b => new HistorialSoftwareViewModel {
            Id = b.id_bp, TipoSolicitud = "Business Pro", Area = b.area, Contacto = b.contacto,
            Telefono = b.telefono, Extension = b.extension, Celular = b.celular, 
            Evidencia = b.evidencia, Puesto = b.puesto, Descripcion = b.descripcion_proceso,
            OrigenTabla = "BusinessPro"
        }).ToListAsync();
        softwareItems.AddRange(bp);

        // 4. Interfaces
        var inter = await _context.Interfaces.Select(i => new HistorialSoftwareViewModel {
            Id = i.id_interfaces, TipoSolicitud = "Interfaces", Contacto = i.contacto,
            Telefono = i.telefono, Extension = i.extension, Celular = i.celular,
            Evidencia = i.evidencia, Puesto = i.puesto, Descripcion = i.descripcion_proceso,
            OrigenTabla = "Interfaces"
        }).ToListAsync();
        softwareItems.AddRange(inter);

        // 5. Incadea
        var inc = await _context.Incadea.Select(i => new HistorialSoftwareViewModel {
            Id = i.id_incadea, TipoSolicitud = "Incadea", Area = i.area, Contacto = i.contacto,
            Telefono = i.telefono, Extension = i.extension, Celular = i.celular,
            Evidencia = i.evidencia, Puesto = i.puesto, Descripcion = i.descripcion_proceso,
            OrigenTabla = "Incadea"
        }).ToListAsync();
        softwareItems.AddRange(inc);

        return softwareItems;
    }
}