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

        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        string usuarioActual = usuarioSesion?.ToLower() ?? "";

        var softwareItems = await GetSoftwareItemsInternalAsync();

        // Filtrar según el rol del usuario
        var filteredItems = softwareItems
            .Where(x => usuarioActual == "jefe0017" || usuarioActual == "gerencia001" || x.AsignadoA?.ToLower() == usuarioActual)
            .OrderByDescending(x => x.Id);

        return View("~/Views/Home/historial_servicio_software.cshtml", filteredItems);
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketsByNomenclatura(string nomenclatura)
    {
        if (string.IsNullOrWhiteSpace(nomenclatura)) return Json(new List<HistorialSoftwareViewModel>());

        var allItems = await GetSoftwareItemsInternalAsync();
        var filtered = allItems.Where(x => x.Nomenclatura == nomenclatura).ToList();
        return Json(filtered);
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketDetails(int id, string origen)
    {
        var allItems = await GetSoftwareItemsInternalAsync();
        var ticket = allItems.FirstOrDefault(t => t.Id == id && t.OrigenTabla == origen);

        if (ticket == null)
        {
            return NotFound();
        }

        // Obtener el historial de detalles para este ticket
        IQueryable<DetalleSoftwareServicio> query = _context.DetalleSoftwareServicios;
        switch (origen)
        {
            case "Altas":
                query = query.Where(d => d.id_alta == id);
                break;
            case "Bajas":
                query = query.Where(d => d.id_baja == id);
                break;
            case "BusinessPro":
                query = query.Where(d => d.id_bp == id);
                break;
            case "Interfaces":
                query = query.Where(d => d.id_interfaces == id);
                break;
            case "Incadea":
                query = query.Where(d => d.id_incadea == id);
                break;
        }

        var history = await query.OrderBy(d => d.FechaRegistro).ToListAsync();

        return Json(new { ticket, history });
    }

    [HttpGet]
    public async Task<IActionResult> GetHistorialStatusSoftware()
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuarioSesion)) return Unauthorized();
        string usuarioActual = usuarioSesion.ToLower();

        var allItems = await GetSoftwareItemsInternalAsync();

        var updates = allItems
            .Where(x => usuarioActual == "jefe0017" || usuarioActual == "gerencia001" || x.AsignadoA?.ToLower() == usuarioActual)
            .Select(u => new {
                IdOrden = u.Id,
                u.OrigenTabla,
                u.Estatus,
                u.AsignadoA
            }).ToList();

        return Json(updates);
    }

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

    [HttpPost]
    public async Task<IActionResult> GuardarCambiosHistorialSoftware([FromBody] List<HistorialSoftwareUpdateDto> items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = false, message = "No se recibieron datos para guardar." });
        }

        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuarioSesion))
        {
            return Unauthorized();
        }
        string usuarioActual = usuarioSesion.ToLower();

        foreach (var item in items)
        {
            DetalleSoftwareServicio? ultimoDetalle = null;
            switch (item.OrigenTabla)
            {
                case "Altas":
                    ultimoDetalle = await _context.DetalleSoftwareServicios.Where(d => d.id_alta == item.IdOrden).OrderByDescending(d => d.Id).FirstOrDefaultAsync();
                    break;
                case "Bajas":
                    ultimoDetalle = await _context.DetalleSoftwareServicios.Where(d => d.id_baja == item.IdOrden).OrderByDescending(d => d.Id).FirstOrDefaultAsync();
                    break;
                case "BusinessPro":
                    ultimoDetalle = await _context.DetalleSoftwareServicios.Where(d => d.id_bp == item.IdOrden).OrderByDescending(d => d.Id).FirstOrDefaultAsync();
                    break;
                case "Interfaces":
                    ultimoDetalle = await _context.DetalleSoftwareServicios.Where(d => d.id_interfaces == item.IdOrden).OrderByDescending(d => d.Id).FirstOrDefaultAsync();
                    break;
                case "Incadea":
                    ultimoDetalle = await _context.DetalleSoftwareServicios.Where(d => d.id_incadea == item.IdOrden).OrderByDescending(d => d.Id).FirstOrDefaultAsync();
                    break;
            }

            // Seguridad: Solo el jefe puede cambiar la asignación
            string? finalAsignado = (usuarioActual == "jefe0017") ? item.AsignadoA : ultimoDetalle?.AsignadoA;

            // Lógica automática: Si se agrega un comentario y el estatus es 'proceso', cambiarlo a 'cerrarlo'.
            if (!string.IsNullOrWhiteSpace(item.Comentarios) && item.Estatus == "proceso")
            {
                item.Estatus = "cerrado";
            }

            // Si no hay detalle previo o si algún campo ha cambiado, se crea un nuevo registro
            if (ultimoDetalle == null || ultimoDetalle.Estatus != item.Estatus || 
            ultimoDetalle.Comentarios != item.Comentarios || ultimoDetalle.AsignadoA != finalAsignado)
            {
                var nuevoDetalle = new DetalleSoftwareServicio 
                { 
                    Estatus = item.Estatus, 
                    Comentarios = item.Comentarios, 
                    AsignadoA = finalAsignado,
                    FechaRegistro = DateTime.Now
                };
                switch (item.OrigenTabla)
                {
                    case "Altas": nuevoDetalle.id_alta = item.IdOrden; break;
                    case "Bajas": nuevoDetalle.id_baja = item.IdOrden; break;
                    case "BusinessPro": nuevoDetalle.id_bp = item.IdOrden; break;
                    case "Interfaces": nuevoDetalle.id_interfaces = item.IdOrden; break;
                    case "Incadea": nuevoDetalle.id_incadea = item.IdOrden; break;
                }
                _context.DetalleSoftwareServicios.Add(nuevoDetalle);
            }
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Cambios guardados correctamente." });
    }
}