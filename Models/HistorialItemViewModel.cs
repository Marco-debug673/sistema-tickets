namespace SistemaTickets.Models
{
    public class HistorialItemViewModel
    {
        public OrdenViewModel Orden { get; set; } = new OrdenViewModel();
        public Detalle? Detalle { get; set; }
    }

    public class OrdenViewModel
    {
        public int id_orden { get; set; }
        public string? nombre_cliente { get; set; }
        public string? nomenclatura { get; set; }
        public string? descripcion { get; set; }
        public string? evidencia { get; set; }
    }

    public class Detalle {
        public int id_orden { get; set; }
        public string? estatus { get; set; }
        public string? comentarios { get; set; }
        public string? asignado_a { get; set; }
    }
}