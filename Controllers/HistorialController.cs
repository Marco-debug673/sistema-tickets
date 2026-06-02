using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class HistorialController : Controller
{
    private readonly AppDbContext _context;

    public HistorialController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> historial_servicio()
    {
        string? usuarioSesion = HttpContext.Session.GetString("Usuario");
        ViewBag.Usuario = usuarioSesion;
        if (string.IsNullOrEmpty(usuarioSesion)) return RedirectToAction("Index", "Home");

        string usuarioActual = usuarioSesion.ToLower();
    
        var itemsQuery = _context.OrdenesServicios
            .Select(o => new HistorialItemViewModel
            {
                Orden = new OrdenViewModel
                {
                    id_orden = o.id_orden,
                    nombre_cliente = o.nombre_cliente,
                    nomenclatura = o.nomenclatura,
                    descripcion = o.descripcion,
                    evidencia = o.evidencia
                },
                Detalle = _context.DetalleOrdenServicios
                    .Where(d => d.id_orden == o.id_orden)
                    .OrderByDescending(d => d.id_detalle)
                    .Select(d => new Detalle
                    {
                        id_orden = d.id_orden,
                        estatus = d.estatus,
                        comentarios = d.comentarios,
                        asignado_a = d.asignado_a
                    })
                    .FirstOrDefault()
            });

        var allItems = await itemsQuery
            .OrderByDescending(x => x.Orden.id_orden)
            .ToListAsync();

        var items = allItems
            .Where(x => usuarioActual == "jefe0018" || x.Detalle?.asignado_a?.ToLower() == usuarioActual)
            .ToList();

        return View("~/Views/Home/historial_servicio.cshtml", items);
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