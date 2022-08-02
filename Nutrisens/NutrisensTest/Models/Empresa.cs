using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreEmpresa { get; set; }

    }
}
