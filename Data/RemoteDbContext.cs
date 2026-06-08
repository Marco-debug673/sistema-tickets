using Microsoft.EntityFrameworkCore;
using SistemaTickets.Models;

namespace SistemaTickets.Data;

public class RemoteDbContext : DbContext
{
    public RemoteDbContext(DbContextOptions<RemoteDbContext> options) 
    : base(options)
    {
    }

    public DbSet<ConActivo> CON_ACTIVOS { get; set; }
    
    // Define aquí los DbSet (tablas) que quieres consultar en la base de datos remota
    // Ejemplo:
    // public DbSet<Incadea> IncadeaRemote { get; set; }
    // public DbSet<BusinessPro> BusinessProRemote { get; set; }
}