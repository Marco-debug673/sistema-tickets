using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models;

[Table("bajas")]
public class Bajas
{
    [Key]
    public int id_baja { get; set; }
    public string nombre { get; set; } = null!;
    public string nomenclatura { get; set; } = null!;
    public string empresa { get; set; } = null!;
    public string sucursal { get; set; } = null!;
    public string puesto { get; set; } = null!;
    public string departamento { get; set; } = null!;
    public string descripcion { get; set; } = null!;
}