using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class OrdenServicio
    {
        [Key]
        public int id_orden { get; set; }
        public string? nombre_cliente { get; set; }
        public string? nomenclatura { get; set; }
        public string? descripcion { get; set; }
        public string? evidencia { get; set; }
        public string? numero_serie { get; set; }
    }
}