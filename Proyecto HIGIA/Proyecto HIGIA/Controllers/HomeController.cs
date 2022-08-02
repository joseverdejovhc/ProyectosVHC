using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proyecto_HIGIA.Data;
using Proyecto_HIGIA.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Proyecto_HIGIA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private string login;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            login = GetCookie();
            int permiso = ComprobarPermiso(login);

            if (permiso != 0)
            {
                return View();
            }
            else
            {
                return View("NoAccess");
            }


        }

        /**
         * Modulo de Presupuestos
         */

        public IActionResult Presupuestos()
        {
            login = GetCookie();

            int permiso = ComprobarPermiso(login);

            if (permiso != 0)
            {
                return View();
            }
            else
            {
                return View("NoAccess");
            }

        }

        public IActionResult getAllCapitulos(int capitulo)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@capitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = capitulo });

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_CAPITULOS_SUBCAPITULOS", parameters);

            var dt_capitulos = dataSet?.Tables?[0];
            var dt_subcapitulos = dataSet?.Tables?[1];

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "dd/MM/yyyy";

            var capitulos = JsonConvert.SerializeObject(dt_capitulos, jsonSettings);
            var subcapitulos = JsonConvert.SerializeObject(dt_subcapitulos, jsonSettings);

            response["capitulos"] = capitulos;
            response["subcapitulos"] = subcapitulos;

            response["success"] = true;

            return Content(response.ToString(), "application/json");

        }
        public IActionResult SaveImporte(int subcapitulo, string importe)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@importe", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = decimal.Parse(importe, new NumberFormatInfo() { NumberDecimalSeparator = "." }) });
            parameters.Add(new SqlParameter("@subcapitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = subcapitulo });
            parameters.Add(new("@mensaje", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });
            parameters.Add(new("@success", SqlDbType.Int) { Direction = ParameterDirection.Output });

            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_GUARDAR_IMPORTE_SUBCAPITULO", parameters);

            if (command.Parameters["@success"].Value != null)
            {
                response["success"] = int.Parse(command.Parameters["@success"].Value.ToString());
                response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();
            }

            return Content(response.ToString(), "application/json");
        }

        /**
         * Modulo de Pedidos
         */

        public IActionResult Pedidos()
        {
            login = GetCookie();

            int permiso = ComprobarPermiso(login);

            if (permiso != 0)
            {
                return View();
            }
            else
            {
                return View("NoAccess");
            }
        }

        public IActionResult getAllPedidos()
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_PEDIDOS", parameters);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "dd/MM/yyyy";

            var dt_pedidos = dataSet?.Tables?[0];
            var pedidos = JsonConvert.SerializeObject(dt_pedidos, jsonSettings);

            response["pedidos"] = pedidos;

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }

        public IActionResult getPedido(int pedido)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@num_pedido", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = pedido });

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_PEDIDO", parameters);
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-dd";

            var dt_pedido = dataSet?.Tables?[0];
            var obj_pedido = JsonConvert.SerializeObject(dt_pedido, jsonSettings); //, jsonSettings
            response["pedido"] = obj_pedido;

            var dt_subcapitulos = dataSet?.Tables?[1];
            var subcapitulos = JsonConvert.SerializeObject(dt_subcapitulos, jsonSettings);
            response["subcapitulos"] = subcapitulos;

            var dt_facturas = dataSet?.Tables?[2];
            var facturas = JsonConvert.SerializeObject(dt_facturas, jsonSettings);
            var archivos = listaArchivosPedido(pedido);
            response["facturas"] = facturas;
            response["archivos"] = JsonConvert.SerializeObject(archivos);

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }

        public FileResult downloadFile(string name, int pedido)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes("O:/ArchivosAdjuntosHigiaTest/" + pedido + "/" + name);
            string fileName = name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        private JArray listaArchivosPedido(int pedido)
        {

            var directoryName = "O:/ArchivosAdjuntosHigiaTest/" + pedido;
            Directory.CreateDirectory(directoryName);


            DirectoryInfo di = new DirectoryInfo("O:/ArchivosAdjuntosHigiaTest/" + pedido);
            FileInfo[] files = di.GetFiles();


            List<string> filesName = new List<string>();
            JArray array = new JArray();
            foreach (FileInfo file in files)
            {
                JObject item = new JObject();
                item["name"] = file.Name;
                array.Add(item);
            }

            return array;
        }

        /** 
         * Modulo de Nuevo Pedido
         */

        public IActionResult NuevoPedido()
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();

            var operacion = "INSERT";
            parameters.Add(new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = operacion });
            parameters.Add(new("@mensaje", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });
            parameters.Add(new("@success", SqlDbType.Int) { Direction = ParameterDirection.Output });
            parameters.Add(new("@identity", SqlDbType.Int) { Direction = ParameterDirection.Output });


            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_CRUD_PEDIDO", parameters);
            Pedido pedido = new Pedido();
            pedido.id = int.Parse(command.Parameters["@identity"].Value.ToString());

            return View("NuevoPedido", pedido);
        }

        public IActionResult getNuevosSubcapitulos(int pedido, int capitulo)
        {
            var response = new JObject();
            var mensaje = "";
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@capitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = capitulo });
            parameters.Add(new SqlParameter("@fk_pedido", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = pedido });

            SqlDataReader reader = BaseDataAccess.GetDataReader("SP_GET_NUEVOS_SUBCAPITULOS", parameters);


            List<Subcapitulo> subcapitulos = new List<Subcapitulo>();

            while (reader.Read())
            {
                mensaje = reader["mensaje"].ToString();

                if (mensaje == "")
                {
                    Subcapitulo subcapitulo = new Subcapitulo();
                    subcapitulo.id = int.Parse(reader["id"].ToString());
                    subcapitulo.cod_subcapitulo = reader["cod_subcapitulo"].ToString();
                    subcapitulo.cod_capitulo = reader["cod_capitulo"].ToString();
                    subcapitulo.nom_capitulo = reader["nom_capitulo"].ToString();
                    subcapitulo.nom_subcapitulo = reader["nom_subcapitulo"].ToString();
                    subcapitulos.Add(subcapitulo);
                }
            }
            reader.Close();

            if (mensaje == "")
            {
                response["subcapitulos"] = JsonConvert.SerializeObject(subcapitulos);

                response["success"] = true;
            }
            else
            {
                response["success"] = false;
                response["mensaje"] = mensaje;
            }


            return Content(response.ToString(), "application/json");
        }

        public IActionResult CRUDSubcapituloPedido(string operacion, int pedido, int subcapitulo, string importe)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();

            if (importe == null)
            {
                importe = "0";
            }

            parameters.Add(new SqlParameter("@importe", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = decimal.Parse(importe, new NumberFormatInfo() { NumberDecimalSeparator = "." }) });
            parameters.Add(new SqlParameter("@subcapitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = subcapitulo });
            parameters.Add(new SqlParameter("@pedido", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = pedido });
            parameters.Add(new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = operacion });
            parameters.Add(new SqlParameter("@usu_mod", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = usuario });

            parameters.Add(new("@mensaje", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });
            parameters.Add(new("@success", SqlDbType.Int) { Direction = ParameterDirection.Output });

            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_CRUD_SUBCAPITULO_PEDIDO", parameters);

            if (command.Parameters["@success"].Value != null)
            {
                response["success"] = int.Parse(command.Parameters["@success"].Value.ToString());
                response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();
            }

            return Content(response.ToString(), "application/json");

        }

        public IActionResult CRUDPedido(string operacion, int id_pedido, string fecha_pedido, string cod_proveedor, string nom_proveedor, string cod_acc_proveedor)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();

            parameters.Add(new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = operacion });
            parameters.Add(new SqlParameter("@id_pedido", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = id_pedido });
            parameters.Add(new SqlParameter("@fecha_pedido", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = fecha_pedido });
            parameters.Add(new SqlParameter("@cod_proveedor", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = cod_proveedor });
            parameters.Add(new SqlParameter("@nom_proveedor", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = nom_proveedor });
            parameters.Add(new SqlParameter("@cod_acc_proveedor", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = cod_acc_proveedor });
            parameters.Add(new SqlParameter("@usu_mod", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = usuario });

            parameters.Add(new("@mensaje", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });
            parameters.Add(new("@success", SqlDbType.Int) { Direction = ParameterDirection.Output });
            parameters.Add(new("@identity", SqlDbType.Int) { Direction = ParameterDirection.Output });

            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_CRUD_PEDIDO", parameters);

            if (command.Parameters["@success"].Value != null)
            {
                response["success"] = int.Parse(command.Parameters["@success"].Value.ToString());
                response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();
            }

            return Content(response.ToString(), "application/json");
        }

        public IActionResult getProveedores()
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_PROVEEDORES", parameters);

            var dt_proveedores = dataSet?.Tables?[0];
            var proveedores = JsonConvert.SerializeObject(dt_proveedores);

            response["proveedores"] = proveedores;

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }


        [HttpPost]
        public IActionResult FileUpload(List<IFormFile> files, int pedido)
        {
            var response = new JObject();

            long size = files.Sum(f => f.Length);

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // full path to file in temp location
                    //var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
                    var filePath = "O:/ArchivosAdjuntosHigiaTest/" + pedido + "/" + formFile.FileName;
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        formFile.CopyToAsync(stream);
                    }
                }
            }

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }


        public IActionResult DeleteFile(string name, int pedido)
        {
            var response = new JObject();

            var filePath = "O:/ArchivosAdjuntosHigiaTest/" + pedido + "/" + name;

            System.IO.File.Delete(filePath);

            response["success"] = true;
            response["mensaje"] = "Archivo borrado correctamente";

            return Content(response.ToString(), "application/json");
        }

        /** 
         * Nueva Factura
         */

        public IActionResult CRUDFactura(string operacion, string num_factura, string num_expediente, string fecha_factura, int pedido, int id_factura, string subcapitulos)
        {
            var response = new JObject();
            var parameters = new List<SqlParameter>();
            var usuario = GetCookie();
            parameters.Add(new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = operacion });
            parameters.Add(new SqlParameter("@num_factura", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = num_factura });
            parameters.Add(new SqlParameter("@num_expediente", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = num_expediente });
            parameters.Add(new SqlParameter("@fecha_factura", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = fecha_factura });
            parameters.Add(new SqlParameter("@usu_mod", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = usuario });

            parameters.Add(new SqlParameter("@pedido", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = pedido });
            parameters.Add(new SqlParameter("@id_factura", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id_factura });

            parameters.Add(new("@mensaje", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });
            parameters.Add(new("@success", SqlDbType.Int) { Direction = ParameterDirection.Output });
            parameters.Add(new("@identificador", SqlDbType.Int) { Direction = ParameterDirection.Output });
            SqlCommand command = BaseDataAccess.ExecuteScalar("SP_CRUD_FACTURA", parameters);

            if (command.Parameters["@success"].Value != null)
            {
                int success = int.Parse(command.Parameters["@success"].Value.ToString());
                response["success"] = success;
                response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();
                if (success == 1 && (operacion == "UPDATE" || operacion == "INSERT"))
                {
                    int factura = int.Parse(command.Parameters["@identificador"].Value.ToString());
                    JArray array = JArray.Parse(subcapitulos);
                    foreach (JObject item in array) // <-- Note that here we used JObject instead of usual JProperty
                    {
                        parameters = new List<SqlParameter>();
                        parameters.Add(new SqlParameter("@subcapitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = int.Parse(item.GetValue("id_subcapitulo").ToString()) });
                        parameters.Add(new SqlParameter("@factura", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = factura });
                        if (item.GetValue("facturado") != null && item.GetValue("facturado").ToString() != "")
                        {
                            parameters.Add(new SqlParameter("@importe", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = Double.Parse(item.GetValue("facturado").ToString()) });
                        }
                        else
                        {
                            parameters.Add(new SqlParameter("@importe", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = 0 });

                        }

                        parameters.Add(new SqlParameter("@pedido", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = pedido });
                        if (item.GetValue("id") != null && item.GetValue("id").ToString() != "")
                        {
                            parameters.Add(new SqlParameter("@factura_subcapitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = int.Parse(item.GetValue("id").ToString()) });
                        }

                        parameters.Add(new("@mensaje", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });
                        parameters.Add(new("@success", SqlDbType.Int) { Direction = ParameterDirection.Output });
                        SqlCommand command_subcapitulo = BaseDataAccess.ExecuteScalar("SP_GUARDAR_SUBCAPITULO_FACTURA", parameters);
                        success = int.Parse(command_subcapitulo.Parameters["@success"].Value.ToString());
                        if (success == 0)
                        {
                            response["success"] = success;
                            response["mensaje"] = command_subcapitulo.Parameters["@mensaje"].Value.ToString();
                        }

                    }
                }

            }

            return Content(response.ToString(), "application/json");
        }

        public IActionResult getSubcapitulosFactura(int factura)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@factura", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = factura });

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_SUBCAPITULOS_FACTURA", parameters);

            var dt_subcapitulos = dataSet?.Tables?[0];
            var subcapitulos = JsonConvert.SerializeObject(dt_subcapitulos);

            response["subcapitulos"] = subcapitulos;

            response["success"] = true;

            return Content(response.ToString(), "application/json");

        }

        /**
         *  Módulo de Facturas
         */

        public IActionResult Facturas()
        {
            login = GetCookie();

            int permiso = ComprobarPermiso(login);

            if (permiso != 0)
            {
                return View();
            }
            else
            {
                return View("NoAccess");
            }
        }

        public IActionResult getFacturas()
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_FACTURAS", parameters);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-dd";


            var dt_facturas = dataSet?.Tables?[0];
            var facturas = JsonConvert.SerializeObject(dt_facturas, jsonSettings);

            response["facturas"] = facturas;

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }

        /**
         * Seguimiento Presupuesto
         */

        public IActionResult SeguimientoPresupuesto()
        {
            login = GetCookie();

            int permiso = ComprobarPermiso(login);

            if (permiso != 0)
            {
                return View();
            }
            else
            {
                return View("NoAccess");
            }
        }

        public IActionResult getSeguimiento(int capitulo)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@capitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = capitulo });

            var dataSet = BaseDataAccess.GetDataSet("SP_SEGUIMIENTO_CAPITULOS", parameters);

            var dt_capitulos = dataSet?.Tables?[0];
            var capitulos = JsonConvert.SerializeObject(dt_capitulos);

            var dt_subcapitulos = dataSet?.Tables?[1];
            var subcapitulos = JsonConvert.SerializeObject(dt_subcapitulos);

            response["capitulos"] = capitulos;
            response["subcapitulos"] = subcapitulos;

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }

        public IActionResult getDesglosePedidos(int capitulo)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@capitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = capitulo });

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_DESGLOSE_PEDIDOS", parameters);

            var dt_desglose = dataSet?.Tables?[0];
            var desglose = JsonConvert.SerializeObject(dt_desglose);

            response["desglose"] = desglose;

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }

        public IActionResult getDesgloseFacturas(int capitulo)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@capitulo", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = capitulo });

            var dataSet = BaseDataAccess.GetDataSet("SP_GET_DESGLOSE_FACTURAS", parameters);

            var dt_desglose = dataSet?.Tables?[0];
            var desglose = JsonConvert.SerializeObject(dt_desglose);

            response["desglose"] = desglose;

            response["success"] = true;

            return Content(response.ToString(), "application/json");
        }

        private string GetCookie()
        {
            if (Request.Cookies["credencialesIntranetMVC"] != null && Request.Cookies["credencialesIntranetMVC"] != "")
            {
                login = Request.Cookies["credencialesIntranetMVC"].ToString();
            }
            else
            {
                Response.Redirect("http://dev.vegenat.net/Intranet2/Login?ReturnUrl=dev.vegenat.net%2HIGIA%2&out=1");
            }

            return login;
        }
        public int ComprobarPermiso(string login)
        {
            int permiso;
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@usuario", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = login });
            parameters.Add(new SqlParameter("@operacion", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = "COMPROBARPERMISO" });
            parameters.Add(new SqlParameter("@success", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output });


            SqlCommand command = BaseDataAccess.ExecuteScalar("CRUD_USUARIOS", parameters);

            if (command.Parameters["@success"].Value != null)
            {
                permiso = Convert.ToInt16(command.Parameters["@success"].Value);

            }
            else
            {
                permiso = 0;
            }

            return permiso;
        }

        /**
         * Modulo de Usuarios          
         */

        public IActionResult Usuarios()
        {
            login = GetCookie();
            int permiso = ComprobarPermiso(login);

            if (permiso != 0)
            {
                return View();
            }
            else
            {
                return View("NoAccess");
            }
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

                usuarios.Add(usuario);
            }
            reader.Close();

            response["success"] = true;
            response["usuarios"] = JsonConvert.SerializeObject(usuarios);

            return Content(response.ToString(), "application/json");

        }

        [HttpPost]
        public IActionResult GuardarUsuario(string login, string nombre, string operacion)
        {
            var response = new JObject();
            response["success"] = false;

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = operacion });
            parameters.Add(new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login });
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        public IActionResult LeerExcel()
        {
            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open("C:/Users/jverdejo/Desktop/higiatest.xlsx", false))
                {
                    var capitulo = new Capitulo();

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();
                    StringBuilder excelResult = new StringBuilder();
                    //using for each loop to get the sheet from the sheetcollection  
                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();
                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            //https://localhost:7116/Home/LeerExcel
                            if (int.Parse(thecurrentrow.RowIndex) >= 4)
                            {
                                var subcapitulo = new Subcapitulo();
                                var tipo = "";
                                foreach (Cell thecurrentcell in thecurrentrow)
                                {

                                    //statement to take the integer value  
                                    string currentcellvalue = string.Empty;
                                    if (thecurrentcell.DataType != null)
                                    {
                                        if (thecurrentcell.DataType == CellValues.SharedString)
                                        {
                                            int id;
                                            if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                            {
                                                var column = thecurrentcell.CellReference.ToString();
                                                SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                                var value = "";
                                                if (item.Text != null)
                                                {
                                                    value = item.Text.Text;
                                                }

                                                if (column.Contains("A"))
                                                {
                                                    tipo = value;
                                                }

                                                if (column.Contains("A") && value.Contains("Capítulo"))
                                                {
                                                    capitulo = new Capitulo();
                                                }

                                                if (column.Contains("A") && value.Contains("Subcapítulo"))
                                                {
                                                    subcapitulo = new Subcapitulo();
                                                }

                                                if (column.Contains("B"))
                                                {
                                                    if (tipo.Contains("Capítulo"))
                                                    {
                                                        capitulo.cod_capitulo = value;
                                                    }

                                                    if (tipo.Contains("Subcapítulo"))
                                                    {
                                                        subcapitulo.cod_subcapitulo = value;
                                                    }

                                                }

                                                if (column.Contains("D"))
                                                {
                                                    if (tipo.Contains("Capítulo"))
                                                    {
                                                        capitulo.nom_capitulo = value;
                                                    }

                                                    if (tipo.Contains("Subcapítulo"))
                                                    {
                                                        subcapitulo.nom_subcapitulo = value;
                                                    }

                                                }

                                                if (column.Contains("L"))
                                                {
                                                    if (tipo.Contains("Subcapítulo"))
                                                    {
                                                        subcapitulo.importe = Double.Parse(value);
                                                    }

                                                }

                                                if (column.Contains("M"))
                                                {
                                                    excelResult.AppendLine();
                                                }

                                            }
                                        }
                                    }

                                }
                                excelResult.AppendLine();

                            }
                            excelResult.Append("");


                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return View();

        }
    }
}