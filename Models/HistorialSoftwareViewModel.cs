using System;

namespace SistemaTickets.Models;

public class HistorialSoftwareViewModel
{
    // Propiedades base para Altas, Bajas y Software
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
    public string? OrigenTabla { get; set; }

    public string? NumeroSerie { get; set; }
    // Propiedades para Hardware y Activo Fijo
    public string? AsignadoA { get; set; }
    public string? Comentarios { get; set; }
    public string? Factura { get; set; }
    public string? FechaCompra { get; set; }
    public string? ImporteCompra { get; set; }
    public string? Ubicacion { get; set; }
    public string? TipoActivo { get; set; }
    public string? VidaUtil { get; set; }
    public string? VidaUtilPendiente { get; set; }
    public string? ValorRecuperacion { get; set; }
    public string? Metodo { get; set; }
    public string? FechaInicioUso { get; set; }
    public string? PorcentajeDepreciacion { get; set; }
    public string? FechaUltimaDepreciacion { get; set; }
    public string? DepreciacionAcumulada { get; set; }
    public string? FechaBaja { get; set; }
    public string? ConceptoBaja { get; set; }
    public string? UsuarioOpe { get; set; }
    public string? FechaOpe { get; set; }
    public string? HoraOpe { get; set; }
}