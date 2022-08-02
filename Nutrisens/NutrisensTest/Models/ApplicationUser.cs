
using Microsoft.AspNetCore.Identity;

namespace Nutrisens.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; }

    }
}
