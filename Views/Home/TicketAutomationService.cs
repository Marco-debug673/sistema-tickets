using Microsoft.EntityFrameworkCore;
using SistemaTickets.Data;
using SistemaTickets.Models;

namespace SistemaTickets.Services;

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
            _logger.LogInformation("Revisando tickets con más de 24 horas en estatus 'nuevo'...");
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var limiteTiempo = DateTime.Now.AddHours(-24);

                // Buscar órdenes cuyo ÚLTIMO detalle sea 'nuevo' y tenga más de 24 horas
                var ticketsParaActualizar = await (from o in context.OrdenesServicios
                                                   join d in context.DetalleOrdenServicios on o.id_orden equals d.id_orden into detalles
                                                   let ultimoDetalle = detalles.OrderByDescending(x => x.id_detalle).FirstOrDefault()
                                                   where ultimoDetalle != null && ultimoDetalle.estatus == "nuevo" && ultimoDetalle.fecha_registro <= limiteTiempo
                                                   select o.id_orden).ToListAsync();

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
                        _logger.LogInformation("Ticket {Id} movido a 'proceso' automáticamente.", idOrden);
                    }
                    await context.SaveChangesAsync();
                }
            }

            // Esperar 1 hora antes de la siguiente revisión
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}