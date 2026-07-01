using Microsoft.EntityFrameworkCore;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Services; // El namespace debería ser Services

public class TicketAutomationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TicketAutomationService> _logger;

    public TicketAutomationService(IServiceProvider serviceProvider, ILogger<TicketAutomationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ProcessHardwareTickets(context, stoppingToken);
                await ProcessSoftwareTickets(context, stoppingToken);
            }

            // Esperar 1 hora antes de la siguiente revisión
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcessHardwareTickets(AppDbContext context, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Revisando tickets de HARDWARE con más de 24 horas en estatus 'nuevo'...");
        var limiteTiempo = DateTime.Now.AddHours(-24);

        // La consulta original tenía un problema al acceder a 'o.fecha_registro'.
        // Esta consulta corregida se enfoca en el último detalle del ticket.
        // Si un ticket no tiene detalles, no puede estar en estado 'nuevo', por lo que no se considera.
        // La lógica `|| (ultimoDetalle == null ...)` se elimina porque un ticket para ser 'nuevo' debe tener al menos un detalle con ese estado.
        // Si se crean tickets sin un detalle inicial, esa lógica debería añadirse en el controlador que crea la OrdenServicio.
         var ticketsParaActualizar = await (from o in context.OrdenesServicios
                                           let ultimoDetalle = context.DetalleOrdenServicios
                                               .Where(d => d.id_orden == o.id_orden)
                                               .OrderByDescending(x => x.id_detalle)
                                               .FirstOrDefault()
                                           where ultimoDetalle != null && ultimoDetalle.estatus == "nuevo" && ultimoDetalle.fecha_registro <= limiteTiempo
                                           select o.id_orden).ToListAsync(stoppingToken);

        if (ticketsParaActualizar.Any())
        {
            foreach (var idOrden in ticketsParaActualizar)
            {
                context.DetalleOrdenServicios.Add(new DetalleOrdenServicio
                {
                    id_orden = idOrden,
                    estatus = "proceso",
                    fecha_registro = DateTime.Now,
                    comentarios = "",
                    asignado_a = ""
                });
                _logger.LogInformation("Ticket de Hardware {Id} movido a 'proceso' automáticamente.", idOrden);
            }
            await context.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task ProcessSoftwareTickets(AppDbContext context, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Revisando tickets de SOFTWARE con más de 24 horas en estatus 'nuevo'...");
        var limiteTiempo = DateTime.Now.AddHours(-24);

        // La consulta original es demasiado compleja para el traductor de EF Core.
        // Esta es una forma alternativa y más robusta de obtener los últimos detalles de cada ticket
        // que cumplen con la condición de ser 'nuevos' y tener más de 24 horas.
        var latestDetailsIds = await context.DetalleSoftwareServicios
            .GroupBy(d => new { d.id_alta, d.id_baja, d.id_bp, d.id_interfaces, d.id_incadea }, (key, group) => new {
                LastId = group.Max(d => d.Id)
            })
            .Select(g => g.LastId)
            .ToListAsync(stoppingToken);

        var ticketsParaActualizar = await context.DetalleSoftwareServicios
            .Where(d => latestDetailsIds.Contains(d.Id) && d.Estatus == "nuevo" && d.FechaRegistro <= limiteTiempo)
            .ToListAsync(stoppingToken);

        if (ticketsParaActualizar.Any())
        {
            foreach (var detalle in ticketsParaActualizar)
            {
                var nuevoDetalle = new DetalleSoftwareServicio
                {
                    Estatus = "proceso",
                    FechaRegistro = DateTime.Now,
                    Comentarios = "",
                    AsignadoA = detalle!.AsignadoA, // Mantiene la asignación si ya existe. El '!' soluciona la advertencia CS8602.
                    id_alta = detalle.id_alta,
                    id_baja = detalle.id_baja,
                    id_bp = detalle.id_bp,
                    id_interfaces = detalle.id_interfaces,
                    id_incadea = detalle.id_incadea
                };
                context.DetalleSoftwareServicios.Add(nuevoDetalle);

                string ticketIdentifier = "";
                if (detalle.id_alta.HasValue) ticketIdentifier = $"Altas-{detalle.id_alta}";
                else if (detalle.id_baja.HasValue) ticketIdentifier = $"Bajas-{detalle.id_baja}";
                else if (detalle.id_bp.HasValue) ticketIdentifier = $"BusinessPro-{detalle.id_bp}";
                else if (detalle.id_interfaces.HasValue) ticketIdentifier = $"Interfaces-{detalle.id_interfaces}";
                else if (detalle.id_incadea.HasValue) ticketIdentifier = $"Incadea-{detalle.id_incadea}";

                _logger.LogInformation("Ticket de Software {Id} movido a 'proceso' automáticamente.", ticketIdentifier);
            }
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}