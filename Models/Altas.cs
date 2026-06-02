using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models;

[Table("altas")]
public class Altas
{
    [Key]
    public int id_alta { get; set; }
    public string nombre { get; set; } = null!;
    public string nomenclatura { get; set; } = null!;
    public string empresa { get; set; } = null!;
    public string sucursal { get; set; } = null!;
    public string puesto { get; set; } = null!;
    public string departamento { get; set; } = null!;
    public string descripcion { get; set; } = null!;
}