using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers;

public class DashboardData
{
    public int Nuevo { get; set; }
    public int Proceso { get; set; }
    public int Cerrado { get; set; }
    public int Total { get; set; }
}

public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    private DashboardData GetStatusCountsData()
    {
        var query = _context.OrdenesServicios
            .Select(o => _context.DetalleOrdenServicios
                .Where(d => d.id_orden == o.id_orden)
                .OrderByDescending(d => d.id_detalle)
                .Select(d => d.estatus)
                .FirstOrDefault() ?? "nuevo");

        var statusCounts = query.ToList()
            .GroupBy(s => s)
            .ToDictionary(g => g.Key, g => g.Count());

        var total = statusCounts.Values.Sum();

        return new DashboardData
        {
            Nuevo = statusCounts.GetValueOrDefault("nuevo", 0),
            Proceso = statusCounts.GetValueOrDefault("proceso", 0),
            Cerrado = statusCounts.GetValueOrDefault("cerrado", 0),
            Total = total
        };
    }

    public IActionResult Dashboard()
    {
        string? usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuario)) return RedirectToAction("Index", "Home");
        ViewBag.Usuario = usuario;

        var data = GetStatusCountsData();
        ViewBag.Nuevo = data.Nuevo;
        ViewBag.Proceso = data.Proceso;
        ViewBag.Cerrado = data.Cerrado;
        ViewBag.Total = data.Total;

        return View("~/Views/Home/dashboard.cshtml");
    }

    public IActionResult Dashboard_software()
    {
        string? usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuario)) return RedirectToAction("Index", "Home");
        ViewBag.Usuario = usuario;

        var data = GetStatusCountsData();
        ViewBag.Nuevo = data.Nuevo;
        ViewBag.Proceso = data.Proceso;
        ViewBag.Cerrado = data.Cerrado;
        ViewBag.Total = data.Total;

        return View("~/Views/Home/dashboard_software.cshtml");
    }

    [HttpGet]
    public IActionResult GetDashboardData()
    {
        return Json(GetStatusCountsData());
    }

    [HttpGet]
    public IActionResult GetTicketsByStatus(string status)
    {
        var tickets = _context.OrdenesServicios
            .Select(o => new
            {
                IdOrden = o.id_orden,
                Cliente = o.nombre_cliente,
                Nomenclatura = o.nomenclatura,
                Descripcion = o.descripcion,
                Estatus = _context.DetalleOrdenServicios
                    .Where(d => d.id_orden == o.id_orden)
                    .OrderByDescending(d => d.id_detalle)
                    .Select(d => d.estatus)
                    .FirstOrDefault() ?? "nuevo",
                Comentarios = _context.DetalleOrdenServicios
                    .Where(d => d.id_orden == o.id_orden)
                    .OrderByDescending(d => d.id_detalle)
                    .Select(d => d.comentarios)
                    .FirstOrDefault() ?? ""
            })
            .Where(x => x.Estatus == status)
            .ToList();

        return Json(tickets);
    }
}