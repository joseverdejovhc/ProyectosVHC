
using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class AccionesAE
    {
        [Key]
        public Int16 Id { get; set; }

        public string Accion { get; set; }
    }
}
