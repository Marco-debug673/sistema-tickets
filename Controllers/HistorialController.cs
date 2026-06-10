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

        var activos = await _contextRemote.CON_ACTIVOS.Where(c => c.CAC_ACTIVO == nomenclatura).Select(c => new HistorialSoftwareViewModel {
            Id = 0, TipoSolicitud = "Activo Fijo", Nomenclatura = c.CAC_ACTIVO, Departamento = c.CAC_DEPARTAMENTO, Descripcion = c.CAC_DESCRIPCION, Estatus = c.CAC_SITUACION,
            Factura = c.CAC_FACTURA, FechaCompra = c.CAC_FECHA_COMPRA != null ? c.CAC_FECHA_COMPRA.ToString() : null,
            ImporteCompra = c.CAC_IMPORTE_COMPRA != null ? c.CAC_IMPORTE_COMPRA.ToString() : null,
            Ubicacion = c.CAC_UBICACION, TipoActivo = c.CAC_TIPO, VidaUtil = c.CAC_VIDA_UTIL != null ? c.CAC_VIDA_UTIL.ToString() : null,
            VidaUtilPendiente = c.CAC_VIDA_UTIL_PENDIENTE != null ? c.CAC_VIDA_UTIL_PENDIENTE.ToString() : null,
            ValorRecuperacion = c.CAC_VALOR_RECUPERACION != null ? c.CAC_VALOR_RECUPERACION.ToString() : null,
            Metodo = c.CAC_METODO, FechaInicioUso = c.CAC_FECHA_INICIO_USO != null ? c.CAC_FECHA_INICIO_USO.ToString() : null,
            PorcentajeDepreciacion = c.CAC_PORCENTAJE_DEPRECIACION != null ? c.CAC_PORCENTAJE_DEPRECIACION.ToString() : null,
            FechaUltimaDepreciacion = c.CAC_FECHA_ULTIMA_DEPRECIACION != null ? c.CAC_FECHA_ULTIMA_DEPRECIACION.ToString() : null,
            DepreciacionAcumulada = c.CAC_DEPRECIACION_ACUMULADA != null ? c.CAC_DEPRECIACION_ACUMULADA.ToString() : null,
            FechaBaja = c.CAC_FECHA_BAJA != null ? c.CAC_FECHA_BAJA.ToString() : null,
            ConceptoBaja = c.CAC_CONCEPTO_BAJA, UsuarioOpe = c.CAC_CVEUSU, FechaOpe = c.CAC_FECHOPE != null ? c.CAC_FECHOPE.ToString() : null,
            HoraOpe = c.CAC_HORAOPE
        }).ToListAsync();

        return Json(hardware.Concat(activos).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> GetDetallesActivoFijo(string nomenclatura)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nomenclatura)) return Json(new List<object>());
            nomenclatura = nomenclatura.Trim();

            var resultados = await _contextRemote.CON_ACTIVOS
                .Where(c => c.CAC_ACTIVO.StartsWith(nomenclatura))
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