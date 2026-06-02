using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }
        public string? nombre_usuario { get; set; }
        public string? contrasena { get; set; }
    }
}