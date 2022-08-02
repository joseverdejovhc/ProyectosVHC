using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrisens.Data.Migrations
{
    public partial class prueba : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListaAplicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreAplicacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaAplicaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListaClientes",
                columns: table => new
                {
                    CodigoCliente = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Empresa = table.Column<short>(type: "smallint", nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaClientes", x => x.CodigoCliente);
                });

            migrationBuilder.CreateTable(
                name: "ListaEmpresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaEmpresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListaEstadoAE",
                columns: table => new
                {
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaEstadoAE", x => x.IdEstado);
                });

            migrationBuilder.CreateTable(
                name: "ListaEstadoAI",
                columns: table => new
                {
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaEstadoAI", x => x.IdEstado);
                });

            migrationBuilder.CreateTable(
                name: "ListaPerfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilTexto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombrePerfil = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaPerfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListaReferencias",
                columns: table => new
                {
                    CodigoRef = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Empresa = table.Column<short>(type: "smallint", nullable: false),
                    NombreRef = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaReferencias", x => x.CodigoRef);
                });

            migrationBuilder.CreateTable(
                name: "ListaSecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreSeccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nivel = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaSecciones", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListaAplicaciones");

            migrationBuilder.DropTable(
                name: "ListaClientes");

            migrationBuilder.DropTable(
                name: "ListaEmpresas");

            migrationBuilder.DropTable(
                name: "ListaEstadoAE");

            migrationBuilder.DropTable(
                name: "ListaEstadoAI");

            migrationBuilder.DropTable(
                name: "ListaPerfiles");

            migrationBuilder.DropTable(
                name: "ListaReferencias");

            migrationBuilder.DropTable(
                name: "ListaSecciones");
        }
    }
}
