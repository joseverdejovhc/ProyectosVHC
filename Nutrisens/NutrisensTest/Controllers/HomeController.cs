using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Nutrisens.Models;
using Nutrisens.Data;
using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Nutrisens.Areas.Identity.Data;
using System.Collections.Generic;
using System.Linq;

namespace Nutrisens.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext dbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private int perfil = 0, nivel = 0, aplicacion, seccion;
        private string login;
        private List<Empresa> listaEmpresas;
        private List<Aplicacion> listaAplicaciones;
        private List<Perfil> listaPerfiles;
        private List<Seccion> listaSecciones;
        private AccesoDatos accesoDatos;


        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext usrctxt)
        {
            _logger = logger;

            _userManager = userManager;

            dbContext = usrctxt;

            accesoDatos = new AccesoDatos();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult Index()
        {
            aplicacion = 0;

            perfil = accesoDatos.GetPerfil(User.Identity.Name, aplicacion);

            ViewData["perfil"] = perfil;

            return View();

        }

        public IActionResult Usuarios()
        {
            aplicacion = 0;
            seccion = 5;
            login = User.Identity.Name;

            perfil = accesoDatos.GetPerfil(login, aplicacion);

            if (perfil < 3)
            {
                nivel = 1;
            }
            else
            {
                nivel = accesoDatos.GetNivel(login, seccion);
            }

            ViewData["nivel"] = nivel;

            if (nivel == 0 || perfil > 2)
            {
                return View("NoAccess");
            }
            else
            {
                return View(ListaUsuarios());
            }
        }

        public IActionResult Claims()
        {
            aplicacion = 1;

            perfil = accesoDatos.GetPerfil(User.Identity.Name, aplicacion);

            ViewData["perfil"] = perfil;

            if (perfil == 0)
            {
                return View("NoAccess");
            }
            else
            {
                return View("Claims/Index");
            }

        }

        private DataTable ListaUsuarios()
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "LISTA-USUARIOS" }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private List<Empresa> ListaEmpresas()
        {
            listaEmpresas = dbContext.ListaEmpresas.FromSqlRaw("LISTA_EMPRESAS").ToList();

            return listaEmpresas;
        }


        public IActionResult ObtenerEmpresasMultiple()
        {
            listaEmpresas = ListaEmpresas();

            return PartialView("EmpresasMultiple", listaEmpresas);
        }

        public string[] ObtenerEmpresasUsuario(string usuario)
        {
            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-EMPRESAS" }
             };

            SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            List<string> lista_empresas = new List<string>();

            while (reader.Read())
            {
                lista_empresas.Add(reader["empresa"].ToString());
            }

            reader.Close();

            return lista_empresas.ToArray();
        }

        [HttpPost]
        public IActionResult GuardarEmpresasUsuario(string operacion, string usuario, string empresas, string usuario_mod)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = operacion },
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario },
             new SqlParameter("@empresas", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = empresas },
             new SqlParameter("@usuario_mod", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario_mod }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            return Content("Companies assigned correctly");
        }


        private List<Aplicacion> ListaAplicaciones()
        {
            listaAplicaciones = dbContext.ListaAplicaciones.FromSqlRaw("LISTA_APLICACIONES").ToList();

            return listaAplicaciones;
        }

        public IActionResult ObtenerAplicaciones()
        {
            listaAplicaciones = ListaAplicaciones();

            return PartialView("Aplicaciones", listaAplicaciones);
        }

        private List<Perfil> ListaPerfiles()
        {
            listaPerfiles = dbContext.ListaPerfiles.FromSqlRaw("LISTA_PERFILES").ToList();

            return listaPerfiles;
        }

        public IActionResult ObtenerPerfiles()
        {
            listaPerfiles = ListaPerfiles();

            return PartialView("Perfiles", listaPerfiles);
        }

        private List<Seccion> ListaSecciones(string aplicacion)
        {
            var parameters = new[] {
             new SqlParameter("@aplicacion", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = aplicacion }
             };

            listaSecciones = dbContext.ListaSecciones.FromSqlRaw("LISTA_SECCIONES @aplicacion={0}", parameters).ToList();

            return listaSecciones;
        }

        public IActionResult ObtenerSecciones(string aplicacion)
        {
            listaSecciones = ListaSecciones(aplicacion);

            return PartialView("Secciones", listaSecciones);
        }

        private List<Seccion> ListaSeccionesUsuario(string aplicacion, string usuario)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-SECCIONES" },
             new SqlParameter("@aplicacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = aplicacion },
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario }
             };

            listaSecciones = dbContext.ListaSecciones.FromSqlRaw("CRUD_USUARIOS @operacion={0}, @aplicacion={1}, @usuario={2}", parameters).ToList();

            return listaSecciones;
        }

        public IActionResult ObtenerSeccionesUsuario(string aplicacion, string usuario)
        {
            listaSecciones = ListaSeccionesUsuario(aplicacion, usuario);

            return PartialView("Secciones", listaSecciones);
        }

        [HttpPost]
        public IActionResult GuardarAccesos(string usuario, string seccion, string nivel, string usuario_mod)
        {
            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario },
             new SqlParameter("@seccion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = seccion },
             new SqlParameter("@nivel", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = nivel },
             new SqlParameter("@usuario_mod", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario_mod }
             };

            accesoDatos.EjecutarProcedimiento("GUARDAR_ACCESOS", CommandType.StoredProcedure, parameters);

            return Content("ok");
        }

        [HttpPost]
        public IActionResult GuardarPerfil(string usuario, string perfil, string aplicacion, string usuario_mod)
        {
            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario },
             new SqlParameter("@aplicacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = aplicacion },
             new SqlParameter("@perfil", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = perfil },
             new SqlParameter("@usuario_mod", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario_mod }
             };

            accesoDatos.EjecutarProcedimiento("GUARDAR_PERFIL", CommandType.StoredProcedure, parameters);

            return Content("ok");
        }

        public IActionResult ObtenerPerfilUsuario(string usuario, string aplicacion)
        {
            var parameters = new[] {
                new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario },
                new SqlParameter("@aplicacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = aplicacion },
                new SqlParameter("@perfil", SqlDbType.Int) {Direction = ParameterDirection.Output}
             };

            dbContext.Database.ExecuteSqlRaw("OBTENER_PERFIL @usuario={0}, @aplicacion={1}, @perfil={2} output", parameters);

            string perfil_usuario = parameters[2].Value.ToString();

            return Json(new { perfil = perfil_usuario });

        }
    }
}