
using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class Referencia
    {
        public Int16 Empresa { get; set; }

        [Key]
        public string CodigoRef { get; set; }

        public string NombreRef { get; set; }
    }
}
