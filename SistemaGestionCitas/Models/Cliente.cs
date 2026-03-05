using System.ComponentModel.DataAnnotations;

namespace SistemaGestionCitas.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; }

        [Required]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Required]
        [Phone]
        public string Telefono { get; set; }

        [EmailAddress]
        public string Correo { get; set; }
    }
}
