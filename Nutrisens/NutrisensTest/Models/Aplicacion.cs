using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class Aplicacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreAplicacion { get; set; }

    }
}
