using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class HistorialController : Controller
{
    private readonly AppDbContext _context;
    private readonly RemoteDbContext _contextRemote;

    public HistorialController(AppDbContext context, RemoteDbContext contextRemote)
    {
        _context = context;
        _contextRemote = contextRemote;
    }

    [HttpGet]
    public async Task<IActionResult> historial_servicio()
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        ViewBag.Usuario = usuarioSesion;
        if (string.IsNullOrEmpty(usuarioSesion)) return RedirectToAction("Index", "Home");

        string usuarioActual = usuarioSesion.ToLower();
    
        var hardwareItems = await _context.OrdenesServicios
            .Select(o => new HistorialSoftwareViewModel
            {
                Id = o.id_orden,
                TipoSolicitud = "Hardware",
                Nombre = o.nombre_cliente,
                Nomenclatura = o.nomenclatura,
                Descripcion = o.descripcion,
                Evidencia = o.evidencia,
                OrigenTabla = "OrdenesServicios",
                Estatus = _context.DetalleOrdenServicios.Where(d => d.id_orden == o.id_orden).OrderByDescending(d => d.id_detalle).Select(d => d.estatus).FirstOrDefault() ?? "nuevo",
                AsignadoA = _context.DetalleOrdenServicios.Where(d => d.id_orden == o.id_orden).OrderByDescending(d => d.id_detalle).Select(d => d.asignado_a).FirstOrDefault() ?? "",
                Comentarios = _context.DetalleOrdenServicios.Where(d => d.id_orden == o.id_orden).OrderByDescending(d => d.id_detalle).Select(d => d.comentarios).FirstOrDefault() ?? ""
            }).ToListAsync();

        var combined = hardwareItems
            .Where(x => usuarioActual == "jefe0018" || usuarioActual == "gerencia001" || x.AsignadoA?.ToLower() == usuarioActual)
            .OrderByDescending(x => x.Id)
            .ToList();

        return View("~/Views/Home/historial_servicio.cshtml", combined);
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketsByNomenclatura(string nomenclatura)
    {
        if (string.IsNullOrWhiteSpace(nomenclatura)) return Json(new List<HistorialSoftwareViewModel>());
        nomenclatura = nomenclatura.Trim();
        
        var hardware = await _context.OrdenesServicios.Where(o => o.nomenclatura == nomenclatura).Select(o => new HistorialSoftwareViewModel {
            Id = o.id_orden, TipoSolicitud = "Hardware", Nombre = o.nombre_cliente, Nomenclatura = o.nomenclatura, Descripcion = o.descripcion, Evidencia = o.evidencia,
            Estatus = _context.DetalleOrdenServicios.Where(d => d.id_orden == o.id_orden).OrderByDescending(d => d.id_detalle).Select(d => d.estatus).FirstOrDefault() ?? "nuevo"
        }).ToListAsync();

        return Json(hardware);
    }

    [HttpGet]
    public async Task<IActionResult> VerificarNomenclatura(string nomenclatura)
    {
        if (string.IsNullOrWhiteSpace(nomenclatura)) return Json(new { exists = false });
        var exists = await _contextRemote.CON_ACTIVOS.AnyAsync(c => c.CAC_ACTIVO == nomenclatura.Trim());
        return Json(new { exists });
    }

    [HttpGet]
    public async Task<IActionResult> GetDetallesActivoFijo(string nomenclatura)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nomenclatura)) return Json(new List<object>());
            nomenclatura = nomenclatura.Trim();

            var resultados = await _contextRemote.CON_ACTIVOS
                .Where(c => c.CAC_ACTIVO == nomenclatura)
                .AsNoTracking() // Mejora rendimiento y evita problemas de tracking
                .ToListAsync();

            return Json(resultados);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetHistorialStatus()
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuarioSesion)) return Unauthorized();
        string usuarioActual = usuarioSesion.ToLower();

        var updatesList = _context.OrdenesServicios
            .Select(o => new
            {
                idOrden = o.id_orden,
                estatus = _context.DetalleOrdenServicios
                    .Where(d => d.id_orden == o.id_orden)
                    .OrderByDescending(d => d.id_detalle)
                    .Select(d => d.estatus)
                    .FirstOrDefault() ?? "nuevo",
                asignadoA = _context.DetalleOrdenServicios
                    .Where(d => d.id_orden == o.id_orden)
                    .OrderByDescending(d => d.id_detalle)
                    .Select(d => d.asignado_a)
                    .FirstOrDefault()
            })
            .ToList();

        var updates = updatesList
            .Where(u => usuarioActual == "jefe0018" || u.asignadoA?.ToLower() == usuarioActual)
            .ToList();

        return Json(updates);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarCambiosHistorial([FromBody] List<HistorialUpdateDto> updates)
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuarioSesion)) return Unauthorized();
        string usuarioActual = usuarioSesion.ToLower();

        if (updates == null || !updates.Any())
            return Json(new { success = false, message = "No hay cambios para guardar." });

        foreach (var update in updates)
        {
            var ultimoDetalle = await _context.DetalleOrdenServicios
                .Where(d => d.id_orden == update.IdOrden)
                .OrderByDescending(d => d.id_detalle)
                .FirstOrDefaultAsync();

            // No procesar actualizaciones para Activos Fijos (IdOrden 0)
            if (update.IdOrden <= 0) continue;

            // Seguridad: El asistente solo puede actualizar lo que tiene asignado
            if (usuarioActual != "jefe0018" && (ultimoDetalle == null || ultimoDetalle.asignado_a?.ToLower() != usuarioActual))
                continue;

            // Seguridad: Solo el jefe puede cambiar la asignación
            string? finalAsignado = (usuarioActual == "jefe0018") ? update.AsignadoA : ultimoDetalle?.asignado_a;

            if (ultimoDetalle == null || ultimoDetalle.estatus != update.Estatus || ultimoDetalle.comentarios != update.Comentarios || ultimoDetalle.asignado_a != finalAsignado)
            {
                _context.DetalleOrdenServicios.Add(new DetalleOrdenServicio
                {
                    id_orden = update.IdOrden,
                    estatus = update.Estatus,
                    comentarios = update.Comentarios,
                    asignado_a = finalAsignado,
                    fecha_registro = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Cambios guardados correctamente." });
    }
}