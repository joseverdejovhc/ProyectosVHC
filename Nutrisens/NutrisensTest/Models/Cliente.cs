using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class Cliente
    {
        public Int16 Empresa { get; set; }

        [Key]
        public string CodigoCliente { get; set; }

        public string NombreCliente { get; set; }
    }
}
