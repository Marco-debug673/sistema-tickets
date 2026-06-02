namespace SistemaTickets.Models
{
    public class HistorialUpdateDto
    {
        public int IdOrden { get; set; }
        public string? Estatus { get; set; }
        public string? Comentarios { get; set; }
        public string? AsignadoA { get; set; }
    }
}