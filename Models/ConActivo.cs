using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models;

[Table("CON_ACTIVOS")]
public class ConActivo
{
    [Key]
    public string CAC_ACTIVO { get; set; } = string.Empty;
    public string? CAC_FACTURA { get; set; }
    public string? CAC_FECHA_COMPRA { get; set; }
    public string? CAC_DESCRIPCION { get; set; }
    
    public decimal? CAC_IMPORTE_COMPRA { get; set; }
    public string? CAC_DEPARTAMENTO { get; set; }
    public string? CAC_UBICACION { get; set; }
    public string? CAC_TIPO { get; set; }
    public decimal? CAC_VIDA_UTIL { get; set; }
    public decimal? CAC_VIDA_UTIL_PENDIENTE { get; set; }
    
    public decimal? CAC_VALOR_RECUPERACION { get; set; }
    public string? CAC_METODO { get; set; }
    public string? CAC_FECHA_INICIO_USO { get; set; }
    
    public decimal? CAC_PORCENTAJE_DEPRECIACION { get; set; }
    public string? CAC_FECHA_ULTIMA_DEPRECIACION { get; set; }
    
    public decimal? CAC_DEPRECIACION_ACUMULADA { get; set; }
    public string? CAC_FECHA_BAJA { get; set; }
    public string? CAC_CONCEPTO_BAJA { get; set; }
    public string? CAC_SITUACION { get; set; }
    public string? CAC_CVEUSU { get; set; }
    public string? CAC_FECHOPE { get; set; }
    public string? CAC_HORAOPE { get; set; }
}