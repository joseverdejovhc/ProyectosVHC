
using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class EstadoAI
    {
        [Key]
        public Int16 IdEstado { get; set; }

        public string NombreEstado { get; set; }

    }
}
