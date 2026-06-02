namespace SistemaTickets.Models
{
    public class HistorialSoftwareViewModel
    {
        public int Id { get; set; }
        public string? TipoSolicitud { get; set; }
        public string? Area { get; set; }
        public string? Nombre { get; set; }
        public string? Contacto { get; set; }
        public string? Nomenclatura { get; set; }
        public string? Empresa { get; set; }
        public string? Sucursal { get; set; }
        public string? Puesto { get; set; }
        public string? Departamento { get; set; }
        public string? Telefono { get; set; }
        public string? Extension { get; set; }
        public string? Celular { get; set; }
        public string? Descripcion { get; set; }
        public string? Evidencia { get; set; }
        public string? Estatus { get; set; }
        public string? Comentarios { get; set; }
        // Propiedad auxiliar para saber de qué tabla viene al guardar
        public string? OrigenTabla { get; set; }
    }
}