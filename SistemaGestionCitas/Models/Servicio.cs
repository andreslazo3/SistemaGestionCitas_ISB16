using System.ComponentModel.DataAnnotations;

namespace SistemaGestionCitas.Models
{
    public class Servicio
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Nombre del Servicio")]        
        public string NombreServicio { get; set; }

        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "Duración (minutos)")]
        public int DuracionMinutos { get; set; }

        [Required]
        public decimal Costo { get; set; }

        public bool Activo { get; set; }

    }
}
