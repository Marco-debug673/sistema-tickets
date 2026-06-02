using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models;

[Table("business_pro")]
public class BusinessPro
{
    [Key]
    public int id_bp { get; set; }
    public string area { get; set; } = null!;
    public string contacto { get; set; } = null!;
    public string telefono { get; set; } = null!;
    public string extension { get; set; } = null!;
    public string celular { get; set; } = null!;
    public string evidencia { get; set; } = null!;
    public string puesto { get; set; } = null!;
    public string descripcion_proceso {get; set; } = null!;
}