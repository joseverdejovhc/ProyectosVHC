using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nutrisens.Areas.Identity.Data;
using Nutrisens.Models;

namespace Nutrisens.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.NombreCompleto)
                .HasMaxLength(250);

        }

        public DbSet<Empresa> ListaEmpresas { get; set; }
        public DbSet<Aplicacion> ListaAplicaciones { get; set; }
        public DbSet<Perfil> ListaPerfiles { get; set; }
        public DbSet<Seccion> ListaSecciones { get; set; }
        public DbSet<Cliente> ListaClientes { get; set; }
        public DbSet<Referencia> ListaReferencias { get; set; }
        public DbSet<EstadoAE> ListaEstadoAE { get; set; }
        public DbSet<EstadoAI> ListaEstadoAI { get; set; }

        public DbSet<AccionesAE> ListaAccionesAE { get; set; }
    }
}