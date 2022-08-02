
using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class Seccion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreSeccion { get; set; }

        public Int16? Nivel { get; set; }

    }
}
