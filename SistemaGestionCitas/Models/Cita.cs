using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;


namespace SistemaGestionCitas.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required(ErrorMessage = "El servicio es obligatorio")]
        [Display(Name = "Servicio")]
        public int ServicioId { get; set; }
        public Servicio? Servicio { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria")]
        [Display(Name = "Hora")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required]
        [Display(Name = "Usuario Responsable")]
        public string UsuarioId { get; set; } = string.Empty;
        public IdentityUser? Usuario { get; set; }

        [Display(Name = "Estado")]
        public EstadoCita Estado { get; set; } = EstadoCita.Programada;
    }

    public enum EstadoCita
    {
        Programada,
        Cancelada
    }
}

