using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models
{
    [Table("DetalleSoftwareServicios")]
    public class DetalleSoftwareServicio
    {
        [Key]
        [Column("id_detalle_software")]
        public int Id { get; set; }
        public string? Estatus { get; set; }
        public string? Comentarios { get; set; }
        [Column("asignado_a")]
        public string? AsignadoA { get; set; }
        [Column("fecha_registro")]
        public DateTime? FechaRegistro { get; set; }
        // Llaves foráneas opcionales
        public int? id_alta { get; set; }
        [ForeignKey("id_alta")]
        public Altas? Alta { get; set; }

        public int? id_baja { get; set; }
        [ForeignKey("id_baja")]
        public Bajas? Baja { get; set; }

        public int? id_interfaces { get; set; }
        [ForeignKey("id_interfaces")]
        public Interfaces? Interface { get; set; }

        public int? id_bp { get; set; }
        [ForeignKey("id_bp")]
        public BusinessPro? BusinessPro { get; set; }

        public int? id_incadea { get; set; }
        [ForeignKey("id_incadea")]
        public Incadea? Incadea { get; set; }
    }
}