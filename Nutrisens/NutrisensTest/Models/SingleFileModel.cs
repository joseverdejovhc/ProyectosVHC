using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class SingleFileModel : ResponseModel
    {
        [Required(ErrorMessage = "Please select file")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "Please select a company")]
        public string Empresa { get; set; }

        [Required(ErrorMessage = "Please select a data load type")]
        public string Tipo { get; set; }
    }
}
