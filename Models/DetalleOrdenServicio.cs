using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class DetalleOrdenServicio
    {
        [Key]
        public int id_detalle { get; set; }
        public int id_orden { get; set; }
        public string? estatus { get; set; }
        public string? comentarios { get; set; }
        public DateTime? fecha_registro { get; set; }
        public string? asignado_a { get; set; }
    }
}