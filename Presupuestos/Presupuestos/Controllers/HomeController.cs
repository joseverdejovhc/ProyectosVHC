using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Presupuestos.Helper;
using Presupuestos.Models;
using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using LAB.Data;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using GemBox.Spreadsheet;
using System.Text;

namespace Presupuestos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private int nivel = 0;
        private string login;
        private static DBContext dbContext;
        private readonly string strConexion;

        public HomeController(DBContext usrctxt, IConfiguration configuration)
        {
            dbContext = usrctxt;
            strConexion = configuration.GetConnectionString("SqlServer");

        }

        public IActionResult Index()
        {
            return View();
        }

        /**
         * SECCIÓN DE USUARIOS 
         */

        public IActionResult Usuarios()
        {
            login = GetCookie(); 

            nivel = ComprobarPermiso(login, 3);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                // listaUsuarios = ListaUsuarios();

                return View();
            }
        }

       
        [HttpPost]
        public IActionResult GuardarUsuario(string login, string nombre, string operacion)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = operacion });
            parameters.Add(new SqlParameter("@login", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login });
            parameters.Add(new SqlParameter("@nombre", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nombre });

            int operation = BaseDataAccess.ExecuteNonQuery("CRUD_USUARIOS", parameters);

            if (operation == -1)
            {
                response["success"] = true;
                switch (operacion)
                {
                    case "INSERT":
                        response["message"] = "Usuario guardado correctamente";
                        break;
                    case "UPDATE":
                        response["message"] = "Usuario actualizado correctamente";
                        break;
                }
            }
            else
            {
                response["success"] = false;
                response["error"] = "El usuario no se ha guardado";
            }
            return Content(response.ToString(), "application/json");
        }


        [HttpPost]
        public IActionResult getListaUsuarios()
        {
            var response = new JObject();

            SqlDataReader reader = BaseDataAccess.GetDataReader("LISTA_USUARIOS", null);

            List<Usuario> usuarios = new List<Usuario>();

            while (reader.Read())
            {
                Usuario usuario = new Usuario();
                usuario.id = int.Parse(reader["id"].ToString());
                usuario.login = reader["login"].ToString();
                usuario.nombre = reader["nombre"].ToString();
                usuario.rol = reader["rol"].ToString();

                usuarios.Add(usuario);
            }
            reader.Close();

            response["success"] = true;
            response["usuarios"] = JsonConvert.SerializeObject(usuarios);

            return Content(response.ToString(), "application/json");

        }

        public IActionResult getListaSecciones()
        {
            var response = new JObject();

            SqlDataReader reader = BaseDataAccess.GetDataReader("LISTA_SECCIONES", null);

            List<Seccion> secciones = new List<Seccion>();

            while (reader.Read())
            {
                Seccion seccion = new Seccion();
                seccion.id = int.Parse(reader["id"].ToString());
                seccion.texto_modulo = reader["texto_modulo"].ToString();
                secciones.Add(seccion);
            }
            reader.Close();


            response["success"] = true;
            response["secciones"] = JsonConvert.SerializeObject(secciones);

            return Content(response.ToString(), "application/json");

        }

        [HttpPost]
        public IActionResult getSeccionesUsuario(int id)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@id_usuario", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id });


            SqlDataReader reader = BaseDataAccess.GetDataReader("OBTENER_ACCESOS", parameters);

            List<Seccion> secciones = new List<Seccion>();

            while (reader.Read())
            {
                Seccion sec = new Seccion();
                sec.id = int.Parse(reader["id"].ToString());
                sec.alias = reader["alias"].ToString();
                sec.escritura = int.Parse(reader["escritura"].ToString());
                secciones.Add(sec);
            }
            reader.Close();


            response["success"] = true;
            response["secciones"] = JsonConvert.SerializeObject(secciones);

            return Content(response.ToString(), "application/json");

        }
        public IActionResult EliminarUsuario(Int64 id)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "DELETE" });
            parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = id });


            int operation = BaseDataAccess.ExecuteNonQuery("CRUD_USUARIOS", parameters);

            if (operation == -1)
            {
                response["success"] = true;
                response["message"] = "El usuario se ha eliminado correctamente";
            }
            else
            {
                response["success"] = false;
                response["error"] = "El usuario no se ha eliminado";
            }

            return Content(response.ToString(), "application/json");
        }

        [HttpPost]
        public IActionResult saveSeccionesUsuario(int id, int margen_bruto, int conf_centros_coste, int usuarios, int sie, int conf_cuenta_contable, int presupuesto, int conf_referencia)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@id_usuario", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id });
            parameters.Add(new SqlParameter("@margen_bruto", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = margen_bruto });
            parameters.Add(new SqlParameter("@conf_centros_coste", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = conf_centros_coste });
            parameters.Add(new SqlParameter("@usuarios", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = usuarios });
            parameters.Add(new SqlParameter("@sie", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = sie });
            parameters.Add(new SqlParameter("@conf_cuenta_contable", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = conf_cuenta_contable });
            parameters.Add(new SqlParameter("@presupuesto", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = presupuesto });
            parameters.Add(new SqlParameter("@conf_referencia", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = conf_referencia });
            int operation = BaseDataAccess.ExecuteNonQuery("GUARDAR_ACCESO", parameters);

            if (operation == -1)
            {
                response["success"] = true;
            }

            return Content(response.ToString(), "application/json");

        }


        /**
         * SECCIÓN DE MARGEN BRUTO
         */

        public IActionResult MargenBruto()
        {
            login = GetCookie();

            nivel = ComprobarPermiso(login, 1);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                return View();
            }
            // return View("MargenBruto");
        }

        /// <summary>
        /// Esta función la utilizaremos tanto para conseguir las referencias necesarias que mostraremos en la pantalla
        /// con los filtros incluidos, como para cargar el margen bruto en la bbdd
        /// </summary>
        /// <param name="ejercicio"></param>
        /// <param name="planta"></param>
        /// <param name="linea"></param>
        /// <param name="tip_ref"></param>
        /// <param name="agrupacion"></param>
        /// <param name="cargar">true -> guardaría en la bbdd, false-> solo devuelve la info a la pantalla</param>
        /// <returns></returns>
        public IActionResult getReferenciasMargenBruto(int ejercicio, string planta, string linea, string tip_ref, int agrupacion, bool cargar)
        {
            var response = new JObject();

            string mensaje = "";
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_CHECK_PRESUPUESTO_APROBADO", parameters);

            while (reader.Read())
            {
                mensaje = reader["mensaje"].ToString();
            }
            reader.Close();


            if (cargar)
            {
                List<SqlParameter> parames = new List<SqlParameter>();
                parames.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                int operacion = BaseDataAccess.ExecuteNonQuery("SP_CLEAR_MARGEN_BRUTO", parames);

                parames = new List<SqlParameter>();
                parames.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                operacion = BaseDataAccess.ExecuteNonQuery("SP_CLEAR_CDF_UNITARIO", parames);
            }

            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@agrupacion", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = agrupacion });
            parameters.Add(new SqlParameter("@planta", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = planta });
            parameters.Add(new SqlParameter("@linea", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = linea });
            parameters.Add(new SqlParameter("@referencia", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = tip_ref });

            reader = BaseDataAccess.GetDataReader("SP_MARGEN_BRUTO", parameters);

            List<Referencia> referencias = new List<Referencia>();


            while (reader.Read())
            {

                // Utilizo el constructor, pero se puede rellenar el objeto atributo a atributo
                Referencia referencia = new Referencia(reader["CODIRE"].ToString(), reader["CODALT"].ToString(), reader["NOMREF"].ToString(), reader["agrupacion"].ToString(), reader["TIPREF"].ToString(), reader["UNISTK"].ToString(), reader["TIPPRD"].ToString(), reader["linea"].ToString(), reader["planta"].ToString(), reader["euros_ventas"].ToString(), reader["kilos_ventas"].ToString(), reader["stock_previsto"].ToString(), reader["kilos_produccion"].ToString(), reader["cdv_almacen"].ToString(), reader["cdf_almacen"].ToString(), reader["cdf"].ToString(), reader["cdv"].ToString());

                if (referencia.kilos_venta == 0)
                {
                    referencia.pmv_kilos = Math.Round(referencia.euros_venta / 1, 4);
                }
                else
                {
                    referencia.pmv_kilos = Math.Round(referencia.euros_venta / referencia.kilos_venta, 4);
                }

                if (referencia.kilos_venta >= referencia.stock_previsto)
                {
                    referencia.kilos_cargo_almacen = Math.Round(referencia.stock_previsto);
                }
                else
                {
                    referencia.kilos_cargo_almacen = Math.Round(referencia.kilos_venta);
                }

                if (reader["FK_AGR"].ToString() != "" && reader["FK_AGR"].ToString() != null)
                {
                    referencia.FK_AGR = int.Parse(reader["FK_AGR"].ToString());
                }

                referencia.cdv_cargo_almacen = Math.Round(referencia.kilos_cargo_almacen * referencia.cdv_almacen);
                referencia.cdf_cargo_almacen = Math.Round(referencia.kilos_cargo_almacen * referencia.cdf_almacen);
                referencia.kilos_cargo_produccion = Math.Round(referencia.kilos_venta - referencia.kilos_cargo_almacen);

                referencia.cdv_cargo_produccion = Math.Round(referencia.kilos_cargo_produccion * referencia.cdv);
                referencia.cdf_cargo_produccion = Math.Round(referencia.kilos_cargo_produccion * referencia.cdf);
                referencia.cdv_total = Math.Round(referencia.cdv_cargo_almacen + referencia.cdv_cargo_produccion);
                referencia.cdf_total = Math.Round(referencia.cdf_cargo_almacen + referencia.cdf_cargo_produccion);

                referencia.cdv_unitario = Math.Round(referencia.cdv_total / referencia.kilos_venta, 4) > 0 ? Math.Round(referencia.cdv_total / referencia.kilos_venta, 4) : 0;
                referencia.cdf_unitario = Math.Round(referencia.cdf_total / referencia.kilos_venta, 4) > 0 ? Math.Round(referencia.cdf_total / referencia.kilos_venta, 4) : 0;
                referencia.margen_bruto = Math.Round(referencia.euros_venta - referencia.cdv_total);
                referencia.porcen_margen = Math.Round((referencia.margen_bruto / referencia.euros_venta) * 100, 2);


                if (cargar)
                {

                    if (Double.IsNaN(referencia.porcen_margen))
                    {
                        referencia.porcen_margen = 0;
                    }

                    saveCargaMargenBruto(referencia, ejercicio);
                }

                referencias.Add(referencia);
            }
            reader.Close();

            if (cargar)
            {
                var usuario = GetCookie();
                List<SqlParameter> parames = new List<SqlParameter>();
                parames.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                parames.Add(new SqlParameter("@usuario", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario });
                parames.Add(new SqlParameter("@tipo", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = "MARGEN_BRUTO" });

                int operacion = BaseDataAccess.ExecuteNonQuery("SP_ACTUALIZAR_CIERRES", parames);

            }
            response["mensaje"] = mensaje;

            response["success"] = true;
            response["referencias"] = JsonConvert.SerializeObject(referencias);

            return Content(response.ToString(), "application/json");

        }

        /// <summary>
        /// Esta función guardará cada referencia con todos los datos necesarios en la tabla
        /// </summary>
        /// <param name="margen_bruto">Objeto de la referencia</param>
        /// <param name="ejercicio"></param>
        /// <returns>-1,0 dependiendo si el proceso se ejecutó correctamente</returns>
        private int saveCargaMargenBruto(Referencia margen_bruto, int ejercicio)
        {
            int operacion = 0;

            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();

            parameters.Add(new SqlParameter("@codire", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = int.Parse(margen_bruto.CODIRE) });
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@cod_alt", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = margen_bruto.CODALT });
            parameters.Add(new SqlParameter("@referencia", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = margen_bruto.NOMREF });
            parameters.Add(new SqlParameter("@tip_referencia", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = margen_bruto.TIPPRD });
            parameters.Add(new SqlParameter("@cod_planta", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = margen_bruto.planta });
            parameters.Add(new SqlParameter("@cod_linea", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = margen_bruto.linea });
            parameters.Add(new SqlParameter("@agrupacion", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = margen_bruto.agrupacion });
            parameters.Add(new SqlParameter("@fk_agrupacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = margen_bruto.FK_AGR });
            parameters.Add(new SqlParameter("@und_stk", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = margen_bruto.UNISTK });
            parameters.Add(new SqlParameter("@kilos_venta", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.kilos_venta });
            parameters.Add(new SqlParameter("@euros_venta", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.euros_venta });
            parameters.Add(new SqlParameter("@pmv_kg", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.pmv_kilos });
            parameters.Add(new SqlParameter("@stock_fin_ejercicio", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.stock_previsto });
            parameters.Add(new SqlParameter("@cdv_almacen", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdv_almacen });
            parameters.Add(new SqlParameter("@cdf_almacen", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdf_almacen });
            parameters.Add(new SqlParameter("@kilos_cargo_almacen", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.kilos_cargo_almacen });
            parameters.Add(new SqlParameter("@cdv_cargo_almacen", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdv_cargo_almacen });
            parameters.Add(new SqlParameter("@cdf_cargo_almacen", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdf_cargo_almacen });
            parameters.Add(new SqlParameter("@kilos_cargo_produccion", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.kilos_cargo_produccion });
            parameters.Add(new SqlParameter("@kilos_produccion", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.kilos_produccion });
            parameters.Add(new SqlParameter("@cdv", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdv });
            parameters.Add(new SqlParameter("@cdf", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdf });
            parameters.Add(new SqlParameter("@cdv_cargo_produccion", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdv_cargo_produccion });
            parameters.Add(new SqlParameter("@cdf_cargo_produccion", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdf_cargo_produccion });
            parameters.Add(new SqlParameter("@cdv_total", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdv_total });
            parameters.Add(new SqlParameter("@cdf_total", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdf_total });
            parameters.Add(new SqlParameter("@cdv_unitario", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdv_unitario });
            parameters.Add(new SqlParameter("@cdf_unitario", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.cdf_unitario });
            parameters.Add(new SqlParameter("@margen_bruto", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.margen_bruto });
            parameters.Add(new SqlParameter("@porcentaje_margen", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto.porcen_margen });
            parameters.Add(new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario });

            operacion = BaseDataAccess.ExecuteNonQuery("SP_CARGA_MARGEN_BRUTO", parameters);


            return operacion;
        }


        /// <summary>
        /// Devuelve la ultima carga del margen bruto y si el cdf unitario de ese ejercicio está cargado
        /// </summary>
        /// <param name="ejercicio"></param>
        /// <returns></returns>
          public IActionResult getUltimaCarga(int ejercicio)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();
            int permiso = 0;

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new("@cargada_por", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });
            parameters.Add(new("@cdf_unitario", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });


            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_COMPROBAR_ULTIMA_CARGA", parameters);

            if (command.Parameters["@cargada_por"].Value != null)
            {
                response["success"] = true;
                response["cargadapor"] = command.Parameters["@cargada_por"].Value.ToString();
                response["cdf_unitario"] = command.Parameters["@cdf_unitario"].Value.ToString();
            }


            return Content(response.ToString(), "application/json");
        }

        /// <summary>
        /// Comprueba si los presupuestos de ventas y producción y el SIE, están cerrados/aprobados
        /// </summary>
        /// <param name="ejercicio"></param>
        /// <returns></returns>
        public IActionResult getPresupuestoAprobados(int ejercicio)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new("@prepto_ventas", SqlDbType.Int) { Direction = ParameterDirection.Output });
            parameters.Add(new("@prepto_produccion", SqlDbType.Int) { Direction = ParameterDirection.Output });
            parameters.Add(new("@sie", SqlDbType.Int) { Direction = ParameterDirection.Output });

            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_COMPROBAR_PRESUPUESTOS", parameters);

            response["success"] = true;

            if (command.Parameters["@prepto_ventas"].Value.ToString() != null && command.Parameters["@prepto_ventas"].Value.ToString() != "")
            {
                response["prepto_ventas"] = Convert.ToInt16(command.Parameters["@prepto_ventas"].Value.ToString());
            }
            else
            {
                response["prepto_ventas"] = null;
            }

            if (command.Parameters["@prepto_produccion"].Value.ToString() != null && command.Parameters["@prepto_produccion"].Value.ToString() != "")
            {
                response["prepto_produccion"] = Convert.ToInt16(command.Parameters["@prepto_produccion"].Value.ToString());
            }
            else
            {
                response["prepto_produccion"] = null;
            }

            if (command.Parameters["@sie"].Value.ToString() != null && command.Parameters["@sie"].Value.ToString() != "")
            {
                response["sie"] = Convert.ToInt16(command.Parameters["@sie"].Value.ToString());
            }
            else
            {
                response["sie"] = null;
            }

            return Content(response.ToString(), "application/json");
        }

        public IActionResult GetPlantasYLineas()
        {
            var response = new JObject();
            response["success"] = false;
            var parameters = new List<SqlParameter>();

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_TIPOS_LINEAS", parameters);

            List<Linea> lineas = new List<Linea>();


            while (reader.Read())
            {
                Linea linea = new Linea();
                linea.codigo = reader["CODMAN"].ToString();
                linea.nombre = reader["NOM"].ToString();
                lineas.Add(linea);
            }
            reader.Close();

            response["lineas"] = JsonConvert.SerializeObject(lineas);


            reader = BaseDataAccess.GetDataReader("SP_GET_TIPOS_PLANTAS", parameters);

            List<Planta> plantas = new List<Planta>();


            while (reader.Read())
            {
                Planta planta = new Planta();
                planta.codigo = reader["TIPMAQ"].ToString();
                planta.nombre = reader["NOM"].ToString();
                plantas.Add(planta);
            }
            reader.Close();

            response["plantas"] = JsonConvert.SerializeObject(plantas);
            response["success"] = true;



            return Content(response.ToString(), "application/json");
        }



        /**
         * Cálculo CDF Unitario
        */

        public IActionResult GetSIECentrosCosteCDF(int ejercicio)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@clasificacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "Coste Directo Fijo" });
            parameters.Add(new SqlParameter("@seccion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "CDF" });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_SIE_CENTRO_COSTE", parameters);


            List<Sie> centros_coste = new List<Sie>();

            string mensaje = "";

            while (reader.Read())
            {
                mensaje = reader["mensaje"].ToString(); 

                if (mensaje == "")
                {
                    Sie sie = new Sie();
                    sie.codigo = reader["CODIGO"].ToString();
                    sie.nom_centro_coste = reader["nombre"].ToString();
                    sie.fk_centro_coste = Convert.ToInt16(reader["fk_centro_coste"].ToString());
                    sie.clasificacion = reader["clasificacion"].ToString();
                    sie.importe = Convert.ToDouble(reader["importe"].ToString());

                    centros_coste.Add(sie);
                }
            }
            reader.Close();

            if (mensaje == "")
            {
                response["success"] = true;
                response["centros_coste"] = JsonConvert.SerializeObject(centros_coste);
            }
            else
            {
                response["mensaje"] = mensaje;

            }

            return Content(response.ToString(), "application/json");
        }
        public IActionResult GetKilosProduccionPlanta(int ejercicio)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = ejercicio });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_KILOS_BY_PLANTA", parameters);


            List<CDFUnitario> cdfunitario = new List<CDFUnitario>();
            string mensaje = "";

            while (reader.Read())
            {
                mensaje = reader["mensaje"].ToString();

                if (mensaje == "")
                {
                    CDFUnitario cdf = new CDFUnitario();
                    cdf.prevision_kilos = Convert.ToDouble(reader["kilos_produccion"].ToString());
                    if (!reader.IsDBNull("cdf_kilos"))
                    {
                        cdf.cdf_kilos = Convert.ToDouble(reader["cdf_kilos"].ToString());
                    }
                    else
                    {
                        cdf.cdf_kilos = 0;
                    }

                    if (!reader.IsDBNull("gasto_repartir"))
                    {
                        cdf.gasto_repartir = Convert.ToDouble(reader["gasto_repartir"].ToString());
                    }
                    else
                    {
                        cdf.gasto_repartir = 0;
                    }

                    if (!reader.IsDBNull("porcentaje_cdf"))
                    {
                        cdf.porcentaje_cdf = Double.Parse(reader["porcentaje_cdf"].ToString());
                    }
                    else
                    {
                        cdf.porcentaje_cdf = 0;
                    }

                    cdf.planta = reader["cod_planta"].ToString();
                    cdfunitario.Add(cdf);
                }
            }
            reader.Close();


            if (mensaje == "")
            {
                response["success"] = true;
                response["plantas"] = JsonConvert.SerializeObject(cdfunitario);
            }
            else
            {
                response["mensaje"] = mensaje;
            }

            return Content(response.ToString(), "application/json");
        }

        public IActionResult cargarCDFUnitario(string plantas, int ejercicio)
        {
            var response = new JObject();
            response["success"] = false;
            JArray array = JArray.Parse(plantas);
            var usuario = GetCookie();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });

            int operacion = BaseDataAccess.ExecuteNonQuery("SP_CLEAR_CDF_UNITARIO", parameters);
            if (operacion == -1)
            {
                foreach (JObject item in array) // <-- Note that here we used JObject instead of usual JProperty
                {
                    parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                    parameters.Add(new SqlParameter("@planta", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = item.GetValue("planta").ToString() });
                    parameters.Add(new SqlParameter("@porcen_cdf", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = Double.Parse(item.GetValue("porcentaje_cdf").ToString()) });
                    parameters.Add(new SqlParameter("@gasto_repartir", SqlDbType.Float) { Direction = ParameterDirection.Input, Value = Double.Parse(item.GetValue("gasto_repartir").ToString()) });
                    parameters.Add(new SqlParameter("@prevision_kilos", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = int.Parse(item.GetValue("prevision_kilos").ToString()) });
                    parameters.Add(new SqlParameter("@cdf_kilos", SqlDbType.Float) { Direction = ParameterDirection.Input, Value = Double.Parse(item.GetValue("cdf_kilos").ToString()) });
                    parameters.Add(new SqlParameter("@usuario", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario });

                    BaseDataAccess.ExecuteNonQuery("CARGAR_CDF_UNITARIO", parameters);

                }

                response["success"] = true;
            }


            List<SqlParameter> parames = new List<SqlParameter>();
            parames.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parames.Add(new SqlParameter("@usuario", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario });
            parames.Add(new SqlParameter("@tipo", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = "CDF" });

            int operation = BaseDataAccess.ExecuteNonQuery("SP_ACTUALIZAR_CIERRES", parames);

            return Content(response.ToString(), "application/json");
        }


        public IActionResult getUltimaCargaCDF(int ejercicio)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_ULTIMA_CARGA_CDF", parameters);

            while (reader.Read())
            {
                response["cargada_por"] = reader["cargada_por"].ToString();
            }
            reader.Close();

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }


        /**
        *  Modulo Presupuestos
        */
        public IActionResult Presupuestos()
        {
            login = GetCookie();

            nivel = ComprobarPermiso(login, 6);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                // listaUsuarios = ListaUsuarios();

                return View();
            }
        }

        public IActionResult getDatosPresupuesto(int ejercicio)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new("@margen_bruto", SqlDbType.Decimal) { Direction = ParameterDirection.Output });
            parameters.Add(new("@coste_estructura", SqlDbType.Decimal) { Direction = ParameterDirection.Output });
            parameters.Add(new("@coste_linea", SqlDbType.Decimal) { Direction = ParameterDirection.Output });
            parameters.Add(new("@coste_directo_fijo", SqlDbType.Decimal) { Direction = ParameterDirection.Output });
            parameters.Add(new("@cdf_cargo_produccion", SqlDbType.Decimal) { Direction = ParameterDirection.Output });
            parameters.Add(new("@cdf_cargo_almacen", SqlDbType.Decimal) { Direction = ParameterDirection.Output });

            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_GET_PRESUPUESTO", parameters);

            response["success"] = true;
            double margen_bruto = 0;
            double coste_estructura = 0;
            double coste_linea = 0;
            double coste_directo_fijo = 0;
            double antes_impuestos = 0;

            if (command.Parameters["@margen_bruto"].Value.ToString() != null && command.Parameters["@margen_bruto"].Value.ToString() != "")
            {
                margen_bruto = Convert.ToDouble(command.Parameters["@margen_bruto"].Value.ToString());
            }
            else
            {
                margen_bruto = 0;
            }

            response["margen_bruto"] = margen_bruto;


            if (command.Parameters["@coste_estructura"].Value.ToString() != null && command.Parameters["@coste_estructura"].Value.ToString() != "")
            {
                coste_estructura = Convert.ToDouble(command.Parameters["@coste_estructura"].Value.ToString());
            }
            else
            {
                coste_estructura = 0;
            }

            response["coste_estructura"] = coste_estructura;


            if (command.Parameters["@coste_linea"].Value.ToString() != null && command.Parameters["@coste_linea"].Value.ToString() != "")
            {
                coste_linea = Convert.ToDouble(command.Parameters["@coste_linea"].Value.ToString());
            }
            else
            {
                coste_linea = 0;
            }

            response["coste_linea"] = coste_linea;

            if (command.Parameters["@coste_directo_fijo"].Value.ToString() != null && command.Parameters["@coste_directo_fijo"].Value.ToString() != "")
            {
                coste_directo_fijo = Convert.ToDouble(command.Parameters["@coste_directo_fijo"].Value.ToString());
            }
            else
            {
                coste_directo_fijo = 0;
            }

            if (command.Parameters["@cdf_cargo_almacen"].Value.ToString() != null && command.Parameters["@cdf_cargo_almacen"].Value.ToString() != "")
            {
                response["cdf_cargo_almacen"] = Convert.ToDouble(command.Parameters["@cdf_cargo_almacen"].Value.ToString());
            }
            else
            {
                response["cdf_cargo_almacen"] = 0;
            }

            if (command.Parameters["@cdf_cargo_produccion"].Value.ToString() != null && command.Parameters["@cdf_cargo_produccion"].Value.ToString() != "")
            {
                response["cdf_cargo_produccion"] = Convert.ToDouble(command.Parameters["@cdf_cargo_produccion"].Value.ToString());
            }
            else
            {
                response["cdf_cargo_produccion"] = 0;
            }



            response["coste_directo_fijo"] = coste_directo_fijo;

            antes_impuestos = margen_bruto - (coste_estructura + coste_linea + coste_directo_fijo);

            response["antes_impuestos"] = antes_impuestos;

            response["despues_impuestos"] = antes_impuestos * 0.75;

            response["gastosCuentasContables"] = getGastosCuentasContables(ejercicio);

            return Content(response.ToString(), "application/json");
        }

        public IActionResult GetCentrosCostePresupuestos(int ejercicio)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@seccion", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = "PRESUPUESTOS" });
            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_SIE_CENTRO_COSTE", parameters);


            List<Sie> centros_coste = new List<Sie>();

            string mensaje = "";

            while (reader.Read())
            {
                mensaje = reader["mensaje"].ToString();

                if (mensaje == "")
                {
                    Sie sie = new Sie();
                    sie.nom_centro_coste = reader["nombre"].ToString();
                    sie.importe = Convert.ToDouble(reader["importe"].ToString());
                    sie.clasificacion = reader["clasificacion"].ToString();


                    centros_coste.Add(sie);
                }
            }
            reader.Close();

            if (mensaje == "")
            {
                response["success"] = true;
                response["centros_coste"] = JsonConvert.SerializeObject(centros_coste);
            }
            else
            {
                response["mensaje"] = mensaje;
            }


            return Content(response.ToString(), "application/json");
        }


        public IActionResult GetCDFPresupuestos(int ejercicio)
        {
            var response = new JObject();
            response["success"] = false;


            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new("@cdf_almacen", SqlDbType.Decimal) { Direction = ParameterDirection.Output });
            parameters.Add(new("@cdf_produccion", SqlDbType.Decimal) { Direction = ParameterDirection.Output });

            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_CDF_VENTA_PRESUPUESTO", parameters);

            if (command.Parameters["@cdf_almacen"].Value.ToString() != null && command.Parameters["@cdf_almacen"].Value.ToString() != "")
            {
                response["cdf_almacen"] = Convert.ToDouble(command.Parameters["@cdf_almacen"].Value.ToString());
            }

            if (command.Parameters["@cdf_produccion"].Value.ToString() != null && command.Parameters["@cdf_produccion"].Value.ToString() != "")
            {
                response["cdf_produccion"] = Convert.ToDouble(command.Parameters["@cdf_produccion"].Value.ToString());
            }

            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_CDF_DEPARTAMENTOS_PRESUPUESTOS", parameters);

            List<Sie> centros_coste = new List<Sie>();


            while (reader.Read())
            {
                Sie sie = new Sie();
                sie.nom_centro_coste = reader["nombre"].ToString();
                sie.importe = Convert.ToDouble(reader["importe"].ToString());

                centros_coste.Add(sie);
            }
            reader.Close();


            response["success"] = true;
            response["centros_coste"] = JsonConvert.SerializeObject(centros_coste);

            return Content(response.ToString(), "application/json");
        }

        private JObject getGastosCuentasContables(int ejercicio)
        {
            var gastos = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_CUENTAS_EBITDA", parameters);

            double total = 0;

            while (reader.Read())
            {
                gastos[reader["CLASIFICACION"].ToString()] = Convert.ToDouble(reader["importe"].ToString());

                total += Convert.ToDouble(reader["importe"].ToString());
            }
            reader.Close();

            gastos["total"] = total;

            return gastos;
        }

        public IActionResult getMargenBrutoAgrupacion(int ejercicio)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_MARGEN_BRUTO_GROUP_BY_AGRUPACION", parameters);

            List<Referencia> agrupaciones = new List<Referencia>();

            while (reader.Read())
            {
                Referencia referencia = new Referencia();

                referencia.agrupacion = reader["agrupacion"].ToString();
                referencia.kilos_venta = int.Parse(reader["kilos_venta"].ToString());
                referencia.euros_venta = int.Parse(reader["euros_venta"].ToString());
                referencia.cdv_total = Convert.ToDouble(reader["cdv_total"].ToString());
                referencia.porcen_margen = Convert.ToDouble(reader["porcentaje"].ToString());
                referencia.margen_bruto = Convert.ToDouble(reader["margen_bruto"].ToString());
                agrupaciones.Add(referencia);
            }
            reader.Close();

            response["agrupaciones"] = JsonConvert.SerializeObject(agrupaciones);
            response["success"] = true;

            return Content(response.ToString(), "application/json");

        }

        public IActionResult getImportesPorClasificacion(int ejercicio)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_CUENTAS_EBITDA", parameters);

            List<Sie> gastos = new List<Sie>();

            while (reader.Read())
            {
                Sie sie = new Sie();

                sie.clasificacion = reader["clasificacion"].ToString();
                sie.importe = Convert.ToDouble(reader["importe"].ToString());
                gastos.Add(sie);
            }
            reader.Close();

            response["gastos"] = JsonConvert.SerializeObject(gastos);
            response["success"] = true;

            return Content(response.ToString(), "application/json");

        }

        public IActionResult getUltimaCargaPresupuesto(int ejercicio)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_ULTIMA_CARGA_PRESUPUESTO", parameters);

            while (reader.Read())
            {
                response["cargada_por"] = reader["cargada_por"].ToString();
            }
            reader.Close();

            response["success"] = true;

            return Content(response.ToString(), "application/json");

        }

        public IActionResult savePresupuesto(int ejercicio, double margen_bruto, double coste_estructura, double coste_linea, double coste_directo_fijo, double antes_impuesto, double despues_impuesto, double ebitda, int aprobado)
        {
            var response = new JObject();

            var usuario = GetCookie();

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@margen_bruto", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = margen_bruto });
            parameters.Add(new SqlParameter("@coste_estructura", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = coste_estructura });
            parameters.Add(new SqlParameter("@coste_linea", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = coste_linea });
            parameters.Add(new SqlParameter("@coste_directo_fijo", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = coste_directo_fijo });
            parameters.Add(new SqlParameter("@antes_impuesto", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = antes_impuesto });
            parameters.Add(new SqlParameter("@despues_impuesto", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = despues_impuesto });
            parameters.Add(new SqlParameter("@ebitda", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = ebitda });
            parameters.Add(new SqlParameter("@aprobado", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = aprobado });
            parameters.Add(new SqlParameter("@usu_mod", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario });

            int operation = BaseDataAccess.ExecuteNonQuery("SP_GUARDAR_PRESUPUESTO", parameters);

            if (operation == -1)
            {

                if (aprobado == 1)
                {
                    List<SqlParameter> parames = new List<SqlParameter>();
                    parames.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                    parames.Add(new SqlParameter("@usuario", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario });
                    parames.Add(new SqlParameter("@tipo", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = "PRESUPUESTO" });

                    operation = BaseDataAccess.ExecuteNonQuery("SP_ACTUALIZAR_CIERRES", parames);
                }
                else
                {
                    List<SqlParameter> parames = new List<SqlParameter>();
                    parames.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                    parames.Add(new SqlParameter("@usuario", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario });
                    parames.Add(new SqlParameter("@tipo", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = "PRESUPUESTO" });
                    parames.Add(new SqlParameter("@eliminar", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = 1 });

                    operation = BaseDataAccess.ExecuteNonQuery("SP_ACTUALIZAR_CIERRES", parames);
                }

                response["success"] = true;
                response["message"] = "Se ha guardado correctamente el presupuesto";
            }
            else
            {
                response["error"] = "No se ha podido guardar la referencia";
            }

            return Content(response.ToString(), "application/json");
        }

        /**
         * Configuración de Referencias 
        */

        public IActionResult ConfiguracionReferencias()
        {
            login = GetCookie();

            nivel = ComprobarPermiso(login, 7);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                // listaUsuarios = ListaUsuarios();

                return View();
            }
        }

        public IActionResult GetReferencias(Int64 agrupacion)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            if (agrupacion != 0)
            {
                parameters.Add(new SqlParameter("@agrupacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = agrupacion });
            }
            else
            {
                parameters.Add(new SqlParameter("@agrupacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = null });
            }

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_CONF_REFERENCIAS", parameters);

            List<Referencia> referencias = new List<Referencia>();

            while (reader.Read())
            {
                Referencia referencia = new Referencia();

                referencia.id = int.Parse(reader["id"].ToString());
                if (reader["FK_AGR"] != DBNull.Value)
                {
                    referencia.FK_AGR = int.Parse(reader["FK_AGR"].ToString());
                }
                else
                {
                    referencia.FK_AGR = 0;
                }
                referencia.CODIRE = reader["CODIRE"].ToString();
                referencia.CODALT = reader["CODALT"].ToString();
                referencia.NOMREF = reader["NOMREF"].ToString();
                referencia.agrupacion = reader["agrupacion"].ToString();

                referencias.Add(referencia);
            }
            reader.Close();

            response["success"] = true;
            response["referencias"] = JsonConvert.SerializeObject(referencias);

            return Content(response.ToString(), "application/json");
        }

        public IActionResult GetReferenciasNoImportadas()
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_REFERENCIAS_NO_IMPORTADAS", parameters);

            List<Referencia> referencias = new List<Referencia>();

            while (reader.Read())
            {
                Referencia referencia = new Referencia();
                referencia.TIPREF = reader["TIPPRD"].ToString();
                referencia.CODIRE = reader["CODIRE"].ToString();
                referencia.CODALT = reader["CODALT"].ToString();
                referencia.NOMREF = reader["NOMREF"].ToString();
                referencia.agrupacion = reader["NOMTIPREF"].ToString();

                referencias.Add(referencia);
            }
            reader.Close();


            response["success"] = true;
            response["referencias"] = JsonConvert.SerializeObject(referencias);

            return Content(response.ToString(), "application/json");
        }

        [HttpPost]
        public IActionResult saveReferencia(int id, int codire, string nom_ref, string cod_alt, int agrupacion)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            if (id != null && id != 0)
            {
                parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "UPDATE" });

            }
            else
            {
                parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "INSERT" });
            }

            parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id });
            parameters.Add(new SqlParameter("@CODALT", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = cod_alt });
            parameters.Add(new SqlParameter("@REFERENCIA", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nom_ref });
            parameters.Add(new SqlParameter("@agrupacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = agrupacion });
            parameters.Add(new SqlParameter("@CODIRE", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = codire });

            int operation = BaseDataAccess.ExecuteNonQuery("CRUD_REFERENCIA", parameters);

            if (operation == -1)
            {
                response["success"] = true;
                response["message"] = "Se ha guardado correctamente";
            }
            else
            {
                response["error"] = "No se ha podido guardar la referencia";

            }

            return Content(response.ToString(), "application/json");
        }

        [HttpPost]
        public IActionResult addReferencia(int CODIRE, string cod_alt, string nomref, string agrupacion)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "INSERT" });


            //parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id });
            parameters.Add(new SqlParameter("@CODALT", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = cod_alt });
            parameters.Add(new SqlParameter("@REFERENCIA", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nomref });
            parameters.Add(new SqlParameter("@CODIRE", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = CODIRE });

            int operation = BaseDataAccess.ExecuteNonQuery("CRUD_REFERENCIA", parameters);

            if (operation == -1)
            {
                response["success"] = true;
                response["message"] = "Se ha guardado correctamente";
            }
            else
            {
                response["error"] = "No se ha podido guardar la referencia";

            }

            return Content(response.ToString(), "application/json");
        }

        /**
         * SECCIÓN CONFIGURACIÓN CUENTAS CONTABLES
         */

        public IActionResult ConfiguracionCuentasContables()
        {
            login = GetCookie();

            nivel = ComprobarPermiso(login, 5);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                return View();
            }
            // return View("MargenBruto");
        }

        public IActionResult GetCuentasConfiguradas()
        {
            var response = new JObject();
            response["success"] = false;

            string[] clasificaciones = new string[3] { "Amortizaciones y subvenciones", "Gastos Financieros", "Ingresos Financieros" };

            foreach (string clasificacion in clasificaciones)
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@CLASIFICACION", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = clasificacion });

                SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_CONF_CUENTAS_CONTABLES", parameters);

                List<CuentaContable> cuentas_contables = new List<CuentaContable>();

                while (reader.Read())
                {
                    CuentaContable cuenta = new CuentaContable();
                    cuenta.ID = int.Parse(reader["ID"].ToString());
                    cuenta.NUMERO = reader["NUMERO"].ToString();
                    cuenta.NOMBRE = reader["NOMBRE"].ToString();

                    cuentas_contables.Add(cuenta);
                }
                reader.Close();


                response[clasificacion] = JsonConvert.SerializeObject(cuentas_contables);
            }

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }

        public IActionResult GetCuentasNoConfiguradas()
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_CUENTAS_CONTABLES_NO_CONF", parameters);

            List<CuentaContable> cuentas_contables = new List<CuentaContable>();

            while (reader.Read())
            {
                CuentaContable cuenta = new CuentaContable();
                cuenta.NUMERO = reader["CUENTA"].ToString();
                cuenta.NOMBRE = reader["NOMBRE"].ToString();

                cuentas_contables.Add(cuenta);
            }
            reader.Close();

            response["success"] = true;
            response["cuentas_contables"] = JsonConvert.SerializeObject(cuentas_contables);

            return Content(response.ToString(), "application/json");
        }

        [HttpPost]
        public IActionResult CRUDCuentaContable(int id, int identi_gerapli, string numero, string nombre, string clasificacion, string operacion)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();

            parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id });
            parameters.Add(new SqlParameter("@numero", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = numero });
            parameters.Add(new SqlParameter("@NOMBRE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nombre });
            parameters.Add(new SqlParameter("@CLASIFICACION", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = clasificacion });
            parameters.Add(new SqlParameter("@USUARIO", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario });
            parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = operacion });

            int operation = BaseDataAccess.ExecuteNonQuery("CRUD_CONF_CUENTAS_CONTABLES", parameters);

            if (operation == -1)
            {
                response["success"] = true;
                response["message"] = "Se ha guardado correctamente la configuración";
            }
            else
            {
                response["error"] = "No se ha podido guardar la cuenta";

            }

            return Content(response.ToString(), "application/json");
        }

        /**
         * SECCIÓN CONFIGURACIÓN CENTRO COSTE
         */

        public IActionResult ConfiguracionCentroCoste()
        {
            login = GetCookie();

            nivel = ComprobarPermiso(login, 2);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                return View();
            }
            // return View("MargenBruto");
        }

        public IActionResult GetCentrosConfigurados(string clasificacion)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@CLASIFICACION", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = clasificacion });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_CENTROS_COSTE_CONFIGURADOS", parameters);

            List<CentroCoste> centros_coste = new List<CentroCoste>();

            while (reader.Read())
            {
                CentroCoste centro = new CentroCoste();
                centro.ID = int.Parse(reader["ID"].ToString());
                centro.IDENTI = int.Parse(reader["IDENTI"].ToString());
                centro.CODIGO = reader["CODIGO"].ToString();
                centro.EBITDA = int.Parse(reader["EBITDA"].ToString());
                centro.CLASIFICACION = reader["CLASIFICACION"].ToString();
                centro.NOMBRE = reader["NOMBRE"].ToString();


                centros_coste.Add(centro);
            }
            reader.Close();

            response["success"] = true;
            response["centros_coste"] = JsonConvert.SerializeObject(centros_coste);

            return Content(response.ToString(), "application/json");
        }

        public IActionResult GetCentrosNoConfigurados()
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_CENTROS_NO_CONFIGURADOS", parameters);

            List<CentroCoste> centros_coste = new List<CentroCoste>();

            while (reader.Read())
            {
                CentroCoste centro = new CentroCoste();
                centro.IDENTI = int.Parse(reader["IDENTI"].ToString());
                centro.CODIGO = reader["CODIGO"].ToString();
                centro.NOMBRE = reader["NOMBRE"].ToString();

                centros_coste.Add(centro);
            }
            reader.Close();

            response["success"] = true;
            response["centros_coste"] = JsonConvert.SerializeObject(centros_coste);

            return Content(response.ToString(), "application/json");
        }

        [HttpPost]
        public IActionResult CRUDCentroCoste(int id, int identi, string codigo, string nombre, string clasificacion, int ebitda, string operacion)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();

            parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id });
            parameters.Add(new SqlParameter("@IDENTI", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = identi });
            parameters.Add(new SqlParameter("@CODIGO", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo });
            parameters.Add(new SqlParameter("@NOMBRE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nombre });
            parameters.Add(new SqlParameter("@EBITDA", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ebitda });
            parameters.Add(new SqlParameter("@CLASIFICACION", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = clasificacion });
            parameters.Add(new SqlParameter("@USUARIO", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario });
            parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = operacion });
            parameters.Add(new SqlParameter("@return", SqlDbType.Int) { Direction = ParameterDirection.Output });

            //int operation = BaseDataAccess.ExecuteNonQuery("CRUD_CONF_CENTRO_COSTE", parameters);

            SqlCommand command = BaseDataAccess.ExecuteScalar("CRUD_CONF_CENTRO_COSTE", parameters);

            if (command.Parameters["@return"].Value != null)
            {
                int operation = int.Parse(command.Parameters["@return"].Value.ToString());

                if (operation == -1)
                {
                    response["success"] = true;
                    response["message"] = "Se ha guardado correctamente";
                }
                else
                {
                    response["error"] = "No se ha podido guardar la referencia";

                }
            }

            return Content(response.ToString(), "application/json");
        }

        /**
        * SECCIÓN SIE
        */

        public IActionResult SIE()
        {
            login = GetCookie();

            nivel = ComprobarPermiso(login, 4);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                return View();
            }
        }

        public IActionResult getSIEUltimaCarga(int ejercicio)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            var cargadapor = "";
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@cargadapor", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });


            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_GET_CARGA_SIE", parameters);

            if (command.Parameters["@cargadapor"].Value != null)
            {
                response["success"] = true;
                cargadapor = command.Parameters["@cargadapor"].Value.ToString();
            }

            response["cargadapor"] = cargadapor;
            return Content(response.ToString(), "application/json");
        }

        public IActionResult comprobarPresupuestoAprobado(int ejercicio)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = ejercicio });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_CHECK_PRESUPUESTO_APROBADO", parameters);
            string mensaje = "";

            while (reader.Read())
            {
                mensaje = reader["mensaje"].ToString();
            }
            reader.Close();


            if (mensaje == "" || mensaje == "No se ha realizado el cálculo del CDF Unitario")
            {
                response["aprobado"] = false;
            }
            else
            {
                response["aprobado"] = true;
            }

            return Content(response.ToString(), "application/json");

        }


        public IActionResult GetSIECentrosCoste(int ejercicio, string clasificacion, string operacion = null)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();


            if (operacion == "cargar")
            {
                parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                int operation = BaseDataAccess.ExecuteNonQuery("SP_CLEAR_CDF_UNITARIO", parameters);
            }

            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@clasificacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = clasificacion });
            parameters.Add(new SqlParameter("@seccion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SIE" });
            parameters.Add(new SqlParameter("@cargar", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = operacion });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_SIE_CENTRO_COSTE", parameters);


            List<Sie> centros_coste = new List<Sie>();

            string mensaje = "";

            while (reader.Read())
            {
                mensaje = reader["mensaje"].ToString();

                if (mensaje == "")
                {
                    Sie sie = new Sie();
                    sie.codigo = reader["CODIGO"].ToString();
                    sie.nom_centro_coste = reader["nombre"].ToString();
                    sie.fk_centro_coste = Convert.ToInt16(reader["fk_centro_coste"].ToString());
                    sie.clasificacion = reader["clasificacion"].ToString();
                    sie.importe = Convert.ToDouble(reader["importe"].ToString());

                    centros_coste.Add(sie);

                    if (operacion == "cargar")
                    {
                        saveSIECentroCoste(sie, ejercicio);
                    }
                }
            }
            reader.Close();

            if (operacion == "cargar")
            {
                var usuario = GetCookie();
                List<SqlParameter> parames = new List<SqlParameter>();
                parames.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
                parames.Add(new SqlParameter("@usuario", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = usuario });
                parames.Add(new SqlParameter("@tipo", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = "SIE" });

                int operation = BaseDataAccess.ExecuteNonQuery("SP_ACTUALIZAR_CIERRES", parames);

            }

            if (mensaje == "")
            {
                response["success"] = true;
                response["centros_coste"] = JsonConvert.SerializeObject(centros_coste);
            }
            else
            {
                response["mensaje"] = mensaje;
            }

            return Content(response.ToString(), "application/json");
        }

        public IActionResult GetSIECuentasContables(int ejercicio, string clasificacion, string operacion = null)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@clasificacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = clasificacion });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_SIE_CUENTAS_CONTABLES", parameters);


            List<Sie> centros_coste = new List<Sie>();

            while (reader.Read())
            {
                Sie sie = new Sie();
                sie.codigo = reader["CODIGO"].ToString();
                sie.cuenta = reader["cuenta"].ToString();
                sie.fk_centro_coste = Convert.ToInt16(reader["fk_centro_coste"].ToString());
                sie.nombre_cuenta = reader["cuenta_desc"].ToString();
                sie.coste_inversion = reader["coste_inversion"].ToString();
                sie.nom_centro_coste = reader["nombre"].ToString();
                sie.importe = Convert.ToDouble(reader["importe"].ToString());

                if (operacion == "cargar")
                {
                    saveSIECuentaContable(sie, ejercicio);
                }

                centros_coste.Add(sie);
            }
            reader.Close();

            response["success"] = true;
            response["centros_coste"] = JsonConvert.SerializeObject(centros_coste);

            return Content(response.ToString(), "application/json");
        }

        private int saveSIECentroCoste(Sie sie, int ejercicio)
        {
            int operacion = 0;

            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@centro_coste", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = sie.fk_centro_coste });
            parameters.Add(new SqlParameter("@gasto", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = sie.importe });
            parameters.Add(new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario });

            int operation = BaseDataAccess.ExecuteNonQuery("SP_CARGA_SIE_CENTRO_COSTE", parameters);


            return operacion;
        }

        private int saveSIECuentaContable(Sie sie, int ejercicio)
        {
            int operacion = 0;

            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();

            parameters.Add(new SqlParameter("@ejercicio", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = ejercicio });
            parameters.Add(new SqlParameter("@centro_coste", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = sie.fk_centro_coste });
            parameters.Add(new SqlParameter("@cuenta", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = sie.cuenta });
            parameters.Add(new SqlParameter("@coste_inversion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = sie.coste_inversion });
            parameters.Add(new SqlParameter("@nombre_cuenta", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = sie.nombre_cuenta });
            parameters.Add(new SqlParameter("@gasto", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = sie.importe });
            parameters.Add(new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario });

            int operation = BaseDataAccess.ExecuteNonQuery("SP_CARGA_SIE_CUENTA_CONTABLE", parameters);


            return operacion;
        }


        public IActionResult getUltimoPresuprodAprobado()
        {
            var response = new JObject();
            response["success"] = false;

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_ULTIMO_PRESUPROD_APROBADO", new List<SqlParameter>());

            while (reader.Read())
            {
                response["ejercicio"] = Convert.ToInt16(reader["ejercicio"].ToString());
                response["success"] = true;
            }
            reader.Close();

            return Content(response.ToString(), "application/json");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string GetCookie()
        {
            if (Request.Cookies["credencialesIntranetMVC"] != null && Request.Cookies["credencialesIntranetMVC"] != "")
            {
                login = "vegenat0\\" + Request.Cookies["credencialesIntranetMVC"].ToString();
            }
            else
            {
                Response.Redirect("http://dev.vegenat.net/Intranet2/Login?ReturnUrl=dev.vegenat.net%2presupuestotest%2&out=1");
            }

            return login;
        }



        public int ComprobarPermiso(string login, int seccion)
        {
            var parameters = new List<SqlParameter>();
            int permiso = 0;

            parameters.Add(new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login });
            parameters.Add(new SqlParameter("@seccion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = seccion });
            parameters.Add(new SqlParameter("@nivel", SqlDbType.Int) { Direction = ParameterDirection.Output });


            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_COMPROBAR_PERMISO", parameters);

            if (command.Parameters["@nivel"].Value != null)
            {
                permiso = Convert.ToInt16(command.Parameters["@nivel"].Value);
            }
            else
            {
                permiso=-1;
            }

            return permiso;
        }


        /*     public IActionResult LeerExcel()
             {
                 SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

                 var workbook = ExcelFile.Load("C:/Users/jverdejo/Desktop/clasificacion.xlsx");

                 var sb = new StringBuilder();

                 // Iterate through all worksheets in an Excel workbook.
                 foreach (var worksheet in workbook.Worksheets)
                 {
                     sb.AppendLine();
                     sb.AppendFormat("{0} {1} {0}", new string('-', 25), worksheet.Name);

                     // Iterate through all rows in an Excel worksheet.
                     foreach (var row in worksheet.Rows)
                     {

                         if (Convert.ToInt16(row.Name) >= 3)
                         {
                             sb.AppendLine();
                             Excel exc = new Excel();
                             // Iterate through all allocated cells in an Excel row.
                             foreach (var cell in row.AllocatedCells)
                             {

                                 if (Convert.ToInt16(row.Name) >= 3)
                                 {
                                     if (cell.Column.Name == "A")
                                     {
                                         exc.cod_ref = cell.Value.ToString();
                                     }

                                     if (cell.Column.Name == "B")
                                     {
                                         exc.codire = cell.Value.ToString();

                                     }
                                     if (cell.Column.Name == "C")
                                     {
                                         exc.referencia = cell.Value.ToString();

                                     }
                                     if (cell.Column.Name == "D")
                                     {
                                         int agr = 0;
                                         var referencia = cell.Value.ToString();
                                         switch (referencia)
                                         {
                                             case "Nutricion Medica":
                                                 agr = 1;
                                                 break;
                                             case "Preparados Polvo":
                                                 agr = 2;
                                                 break;
                                             case "Transporte":
                                                 agr = 3;
                                                 break;
                                             case "UHT Botella":
                                                 agr = 4;
                                                 break;
                                             case "UHT Brik":
                                                 agr = 5;
                                                 break;
                                             case "MULTISABOR":
                                                 agr = 6;
                                                 break;
                                             case "Comercializados":
                                                 agr = 7;
                                                 break;
                                             case "Ingrediente Activo":
                                                 agr = 8;
                                                 break;
                                             case "Exclusivo":
                                                 agr = 9;
                                                 break;
                                         }
                                         exc.fk_agr = agr;
                                     }
                                 }
                             }

                             var parameters = new List<SqlParameter>();

                             parameters.Add(new SqlParameter("@CODALT", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = exc.cod_ref });
                             parameters.Add(new SqlParameter("@CODIRE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = exc.codire });
                             parameters.Add(new SqlParameter("@REFERENCIA", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = exc.referencia });
                             parameters.Add(new SqlParameter("@FK_AGR", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = exc.fk_agr });

                             int operation = BaseDataAccess.ExecuteNonQuery("AGREGAR_REFERENCIA", parameters);

                             if (operation == -1)
                             {

                             }

                         }

                     }
                 }
                 return View();

             }*/


    }
}