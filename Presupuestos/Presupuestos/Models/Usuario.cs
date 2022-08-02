using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Presupuestos.Models
{
    public class Usuario
    {
        [Key]
        [Required(ErrorMessage = "Debe introducir el login")]
        public string login { get; set; }

        public Int64 id { get; set; }

        [Required(ErrorMessage = "Debe introducir el nombre")]
        public string nombre { get; set; }

        public string rol { get; set; }

    }
}
