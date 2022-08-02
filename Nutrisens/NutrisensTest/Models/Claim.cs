using System.ComponentModel.DataAnnotations;
using Nutrisens.Areas.Identity.Data;

namespace Nutrisens.Models
{
    public class Claim
    {
        [Display(Name = "Company")]
        [Required(ErrorMessage = "Please select a company")]
        public int Empresa { get; set; }

        [Display(Name = "Claim Code")]
        [Key]
        public string? Codigo { get; set; }

        [Display(Name = "Claim Status")]
        public int Estado { get; set; }

        [Display(Name = "Claim Status")]
        public string EstadoNombre { get; set; }

        [Display(Name = "External Action Status")]
        public string? EstadoAE { get; set; }

        [Display(Name = "Internal Action Status")]
        public string? EstadoAI { get; set; }

        [Display(Name = "Customer Code")]
        [Required(ErrorMessage = "Please select a customer")]
        public string CodigoCliente { get; set; }

        [Display(Name = "Customer Name")]
        public string? NombreCliente { get; set; }

        [Display(Name = "Order Number")]
        [Required(ErrorMessage = "Please enter an Order Number")]
        public string? NumPedido { get; set; }

        [Display(Name = "Reason")]
        [Required(ErrorMessage = "Please select a reason")]
        public string? Motivo { get; set; }

        public string? MensajeGuardado { get; set; }

        public string? UsuarioAlta { get; set; }
        public string? UsuarioAltaNombre { get; set; }
        public string? UsuarioMod { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        [Required(ErrorMessage = "Please enter a date")]
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaMod { get; set; }

        public string[]? ListaUsuariosInformar { get; set; }
    }
}
