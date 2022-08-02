using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class Perfil
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string PerfilTexto { get; set; }

        [Required]
        public string NombrePerfil { get; set; }
    }
}
