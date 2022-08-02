
using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class EstadoAE
    {
        [Key]
        public Int16 IdEstado { get; set; }

        public string NombreEstado { get; set; }

    }
}
