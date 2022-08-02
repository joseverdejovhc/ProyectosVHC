
using System.ComponentModel.DataAnnotations;

namespace Nutrisens.Models
{
    public class MultipleFilesModel : ResponseModel
    {
        [Required(ErrorMessage = "Please select files")]
        public List<IFormFile> Files { get; set; }
    }
}
