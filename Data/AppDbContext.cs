using Microsoft.EntityFrameworkCore;
using SistemaTickets.Models;

namespace SistemaTickets.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<OrdenServicio> OrdenesServicios { get; set; }
        public DbSet<DetalleOrdenServicio> DetalleOrdenServicios { get; set; }
        public DbSet<Altas> Altas { get; set; }
        public DbSet<Bajas> Bajas { get; set; }
        public DbSet<BusinessPro> BusinessPro { get; set; }
        public DbSet<Interfaces> Interfaces { get; set; }
        public DbSet<Incadea> Incadea { get; set; }
    }
}