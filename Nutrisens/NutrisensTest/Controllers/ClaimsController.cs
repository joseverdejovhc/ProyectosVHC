using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nutrisens.Areas.Identity.Data;
using Nutrisens.Data;
using Nutrisens.Models;
using Nutrisens.Services;
using System.Data;
using OfficeOpenXml;
using System.IO;
using Microsoft.AspNetCore.Hosting;


namespace Nutrisens.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private ApplicationDbContext dbContext;
        private readonly string strConexion;
        private readonly UserManager<ApplicationUser> _userManager;
        private int perfil = 0, nivel = 0, aplicacion, seccion;
        private string login, idClaim;
        private AccesoDatos accesoDatos;
        private List<Empresa> listaEmpresas;
        private List<Cliente> listaClientes;
        private List<Referencia> listaReferencias;
        private List<EstadoAE> listaEstadoAE;
        private List<EstadoAI> listaEstadoAI;
        private List<AccionesAE> listaAccionesAE;
        private readonly string EstadoDefault = "Pending Definition";

        public ClaimsController(UserManager<ApplicationUser> userManager, ApplicationDbContext usrctxt, IWebHostEnvironment webHostEnvironment)
        {
            //var builder = WebApplication.CreateBuilder();

            //strConexion = builder.Configuration.GetConnectionString("DefaultConnection");

            strConexion = "Data Source=AURABBDD\\MSSQLSERVER2012;Initial Catalog=GRUPO_NUTRISENS;User ID=sa; Password=Vegenat2011; Connection Timeout=300;";


            _userManager = userManager;

            dbContext = usrctxt;

            aplicacion = 1;

            accesoDatos = new AccesoDatos();

            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        private void Comprobaciones(int seccion)
        {
            login = User.Identity.Name;

            perfil = accesoDatos.GetPerfil(login, aplicacion);

            if (perfil == 1)
            {
                nivel = 1;
            }
            else
            {
                nivel = accesoDatos.GetNivel(login, seccion);
            }
        }

        public IActionResult Uploads()
        {
            Comprobaciones(4);

            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;
            ViewData["seccion"] = 4;
            ViewBag.empresas = ListaEmpresasUsuario(login);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                SingleFileModel model = new SingleFileModel();

                return View(model);
            }
        }

        public IActionResult Claims()
        {
            seccion = 1;

            Comprobaciones(seccion);

            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;
            ViewData["seccion"] = seccion;
            ViewBag.empresas = ListaEmpresasUsuario(login);
            ViewBag.estadosAE = ListaEstadosAE();
            ViewBag.estadosAI = ListaEstadosAI();

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                return View();
            }
        }


        public IActionResult ClaimsDetail(string? codigo)
        {
            seccion = 1;

            Comprobaciones(seccion);

            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;
            ViewData["seccion"] = seccion;
            ViewData["mensaje_guardado"] = TempData["mensaje_guardado"];
            ViewData["tipo_mensaje"] = TempData["tipo_mensaje"];

            ViewBag.empresas = ListaEmpresasUsuario(login);

            List<ApplicationUser> listaUsuarios = new List<ApplicationUser>();

            DataTable dt = ListaUsuarios();
            // UsAMOS LINQ para convertir un datatable en una lista de un modelo
            listaUsuarios = (from DataRow dr in dt.Rows
                             select new ApplicationUser()
                             {
                                 UserName = dr["UserName"].ToString(),
                                 Email = dr["Email"].ToString(),
                                 NombreCompleto = dr["NombreCompleto"].ToString()
                             }).ToList();

            ViewBag.usuarios = listaUsuarios;
            ViewBag.acciones = ListaAccionesAE();


            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                Claim modelo = new Claim();

                if (string.IsNullOrEmpty(codigo))
                {
                    modelo.EstadoAE = EstadoDefault;
                    modelo.EstadoAI = EstadoDefault;
                    modelo.Estado = 0;
                    modelo.EstadoNombre = "Draft";
                    modelo.Codigo = "";
                    modelo.FechaAlta = DateTime.Today;
                }
                else
                {
                    modelo = ObtenerClaim(codigo);
                }

                return View("ClaimsDetail", modelo);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClaimsDetail(Claim modelo)
        {
            if (ModelState.IsValid)
            {
                modelo = GuardarClaim(modelo);

                TempData["mensaje_guardado"] = "Claim saved succesfully";
                TempData["tipo_mensaje"] = "success";

                if (modelo.ListaUsuariosInformar != null)
                {
                    EmailSender Mail = new EmailSender();

                    for (int i = 0; i < modelo.ListaUsuariosInformar.Length; i++)
                    {
                        Mail.Enviar(modelo.ListaUsuariosInformar[i], "New Claim", "The Claim number " + modelo.Codigo + " has been registered. " +
                            "<p><a href='http://dev.vegenat.net/nutrisenstest/Claims/ClaimsDetail?codigo=" + modelo.Codigo + "' target='_blank'>Click here</a></p>");
                    }
                }


            }
            else
            {
                TempData["mensaje_guardado"] = "Model no valid";
                TempData["tipo_mensaje"] = "danger";
            }

            string codigo = modelo.Codigo; // Obtenemos el código de la reclamación que se ha creado

            // En lugar de pasar este código al RedirectToAction podría haberlo hecho con TempData (TempData["claim_creado"] = model), pero así es mejor

            return RedirectToAction("ClaimsDetail", "Claims", new { codigo });
        }

        public IActionResult ExternalActions()
        {
            seccion = 2;

            Comprobaciones(seccion);

            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;
            ViewData["seccion"] = seccion;
            ViewBag.empresas = ListaEmpresasUsuario(login);

            List<ApplicationUser> listaUsuarios = new List<ApplicationUser>();

            DataTable dt = ListaUsuarios();
            // UsAMOS LINQ para convertir un datatable en una lista de un modelo
            listaUsuarios = (from DataRow dr in dt.Rows
                             select new ApplicationUser()
                             {
                                 UserName = dr["UserName"].ToString(),
                                 Email = dr["Email"].ToString(),
                                 NombreCompleto = dr["NombreCompleto"].ToString()
                             }).ToList();

            ViewBag.usuarios = listaUsuarios;

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                return View();
            }
        }

        public IActionResult InternalActions()
        {
            seccion = 3;

            Comprobaciones(seccion);

            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;
            ViewData["seccion"] = seccion;
            ViewBag.empresas = ListaEmpresasUsuario(login);

            List<ApplicationUser> listaUsuarios = new List<ApplicationUser>();

            DataTable dt = ListaUsuarios();
            // UsAMOS LINQ para convertir un datatable en una lista de un modelo
            listaUsuarios = (from DataRow dr in dt.Rows
                             select new ApplicationUser()
                             {
                                 UserName = dr["UserName"].ToString(),
                                 Email = dr["Email"].ToString(),
                                 NombreCompleto = dr["NombreCompleto"].ToString()
                             }).ToList();

            ViewBag.usuarios = listaUsuarios;

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                return View();
            }
        }


        private List<Empresa> ListaEmpresasUsuario(string usuario)
        {
            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario }
             };

            listaEmpresas = dbContext.ListaEmpresas.FromSqlRaw("LISTA_EMPRESAS_USUARIO @usuario={0}", parameters).ToList();

            return listaEmpresas;
        }

        private List<Cliente> ListaClientes(string empresa)
        {
            var parameters = new[] {
             new SqlParameter("@empresa", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = empresa }
             };

            listaClientes = dbContext.ListaClientes.FromSqlRaw("LISTA_CLIENTES @empresa={0}", parameters).ToList();

            return listaClientes;
        }

        private List<Referencia> ListaReferencias(string empresa)
        {
            var parameters = new[] {
             new SqlParameter("@empresa", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = empresa }
             };

            listaReferencias = dbContext.ListaReferencias.FromSqlRaw("LISTA_REFERENCIAS @empresa={0}", parameters).ToList();

            return listaReferencias;
        }

        private List<EstadoAE> ListaEstadosAE()
        {
            listaEstadoAE = dbContext.ListaEstadoAE.FromSqlRaw("SELECT * FROM CLAIMS_ESTADOS_GENERAL").ToList();

            return listaEstadoAE;
        }

        private List<EstadoAI> ListaEstadosAI()
        {
            listaEstadoAI = dbContext.ListaEstadoAI.FromSqlRaw("SELECT * FROM CLAIMS_ESTADOS_GENERAL").ToList();

            return listaEstadoAI;
        }

        private List<AccionesAE> ListaAccionesAE()
        {
            listaAccionesAE = dbContext.ListaAccionesAE.FromSqlRaw("SELECT Id, Accion FROM CLAIMS_TIPO_ACCION").ToList();

            return listaAccionesAE;
        }

        private DataTable ListaUsuarios()
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "LISTA-USUARIOS-APP" },
             new SqlParameter("@aplicacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = 1 }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            return dt;
        }

        [HttpPost]
        public IActionResult UploadFile(SingleFileModel model)
        {
            model.IsResponse = true;

            try
            {
                if (model.File != null && !string.IsNullOrEmpty(model.Empresa))
                {
                    string extension = Path.GetExtension(model.File.FileName);

                    if (extension != ".xls" && extension != ".xlsx")
                    {
                        model.IsSuccess = false;
                        model.Message = "The file must be an Excel file";
                    }
                    else
                    {
                        string tipo = model.Tipo;
                        string empresa = model.Empresa;
                        string nombre_archivo = "";

                        // Hay que indicar la ruta con la unidad en el servidor porque Core ya no incluye Server.MapPath
                        // Si guardásemos en la carpeta del proyecto no habría problema, se podría guardar en wwwroot por ejemplo
                        string path = "";
                        path = "O:/NutrisensTest/Claims/Cargas/" + empresa;
                        var carpetaDestino = new DirectoryInfo(path);

                        if (!carpetaDestino.Exists)
                            carpetaDestino.Create();

                        if (tipo == "C")
                        {
                            nombre_archivo = "Customers";
                        }

                        if (tipo == "R")
                        {
                            nombre_archivo = "References";
                        }

                        nombre_archivo += DateTime.Now.ToString("dd-MM-yyyy");

                        string fileName = nombre_archivo + extension;

                        string fileNameWithPath = Path.Combine(path, fileName);

                        // Copiamos el archivo en su carpeta correspondiente
                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            model.File.CopyTo(stream);


                            /********************* GUARDAR EL CONTENIDO DEL EXCEL EN LA TABLA *****/
                            var excel = new ExcelPackage(stream);
                            var worksheet = excel.Workbook.Worksheets[0];

                            int filas = worksheet.Dimension.End.Row;
                            int columnas = worksheet.Dimension.End.Column;

                            // Si encontramos alguna fila del excel vacía la borramos (algunas si no se eliminan del todo quedan con campos vacíos. Recorrer de atrás adelante para ir borrando)
                            for (int row = filas; row >= 1; row--)
                            {
                                if (excel.Workbook.Worksheets[0].Cells[row, 1].Value == null || string.IsNullOrEmpty(excel.Workbook.Worksheets[0].Cells[row, 1].Value.ToString()))
                                {
                                    excel.Workbook.Worksheets[0].DeleteRow(row);
                                }
                            }


                            var dt = excel.ToDataTable();

                            dt.Columns.Add("Empresa", typeof(System.Int16), empresa);

                            var table = "";

                            if (tipo == "C")
                            {
                                table = "CLIENTES";

                                using (var conn = new SqlConnection(strConexion))
                                {
                                    var bulkCopy = new SqlBulkCopy(conn);
                                    bulkCopy.DestinationTableName = table;
                                    conn.Open();

                                    // En este bloque se hace un mapeo de las columnas del excel para hacerlas corresponder con las columnas de la tabla
                                    foreach (DataColumn sourceColumn in dt.Columns)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            if (string.Equals(sourceColumn.ColumnName, "Empresa", StringComparison.OrdinalIgnoreCase))
                                            {
                                                bulkCopy.ColumnMappings.Add(sourceColumn.ColumnName, "Empresa");
                                                break;
                                            }


                                            if (string.Equals(sourceColumn.ColumnName, "CODE", StringComparison.OrdinalIgnoreCase))
                                            {
                                                bulkCopy.ColumnMappings.Add(sourceColumn.ColumnName, "CodigoCliente");
                                                break;
                                            }


                                            if (string.Equals(sourceColumn.ColumnName, "NAME", StringComparison.OrdinalIgnoreCase))
                                            {
                                                bulkCopy.ColumnMappings.Add(sourceColumn.ColumnName, "NombreCliente");
                                                break;
                                            }
                                        }
                                    }


                                    // Borramos primero los datos que haya en la tabla
                                    accesoDatos.EjecutarProcedimiento("DELETE FROM " + table + " WHERE Empresa=" + empresa, CommandType.Text);

                                    // Aquí se insertan en la tabla las columnas del Excel
                                    bulkCopy.WriteToServer(dt);


                                }
                            }

                            if (tipo == "R")
                            {
                                table = "REFERENCIAS";

                                using (var conn = new SqlConnection(strConexion))
                                {
                                    var bulkCopy = new SqlBulkCopy(conn);
                                    bulkCopy.DestinationTableName = table;
                                    conn.Open();

                                    // En este bloque se hace un mapeo de las columnas del excel para hacerlas corresponder con las columnas de la tabla
                                    foreach (DataColumn sourceColumn in dt.Columns)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            if (string.Equals(sourceColumn.ColumnName, "Empresa", StringComparison.OrdinalIgnoreCase))
                                            {
                                                bulkCopy.ColumnMappings.Add(sourceColumn.ColumnName, "Empresa");
                                                break;
                                            }


                                            if (string.Equals(sourceColumn.ColumnName, "CODE", StringComparison.OrdinalIgnoreCase))
                                            {
                                                bulkCopy.ColumnMappings.Add(sourceColumn.ColumnName, "CodigoRef");
                                                break;
                                            }


                                            if (string.Equals(sourceColumn.ColumnName, "NAME", StringComparison.OrdinalIgnoreCase))
                                            {
                                                bulkCopy.ColumnMappings.Add(sourceColumn.ColumnName, "NombreRef");
                                                break;
                                            }
                                        }
                                    }

                                    // Borramos primero los datos que haya en la tabla
                                    accesoDatos.EjecutarProcedimiento("DELETE FROM " + table + " WHERE Empresa=" + empresa, CommandType.Text);

                                    // Aquí se insertan en la tabla las columnas del Excel
                                    bulkCopy.WriteToServer(dt);

                                }
                            }
                            /******************/

                            // Insertamos el registro de la carga
                            GuardarCarga("CARGAR", User.Identity.Name, tipo, fileName, empresa);
                        }

                        model.IsSuccess = true;
                        model.Message = "File upload successfully";
                    }
                }
                else
                {
                    model.IsResponse = false;
                }
            }
            catch (Exception ex)
            {
                model.IsSuccess = false;
                model.Message = ex.Message;
            }

            seccion = 4;

            Comprobaciones(seccion);

            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;
            ViewData["seccion"] = seccion;
            ViewBag.empresas = ListaEmpresasUsuario(login);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                return View("Uploads", model);
            }


        }

        private void GuardarCarga(string operacion, string usuario, string tipo, string nombre_archivo, string empresa)
        {
            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = usuario },
             new SqlParameter("@empresa", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = empresa },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = operacion },
             new SqlParameter("@tipo_archivo", SqlDbType.Char) { Direction = ParameterDirection.Input, Value = tipo },
             new SqlParameter("@nombre_archivo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nombre_archivo }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_CARGAS", CommandType.StoredProcedure, parameters);

        }

        // Actualmente no uso este método, lo guardo para posibles referencias
        public List<FileModel> ListaArchivos()
        {
            //Fetch all files in the Folder (Directory).
            string[] filePaths = Directory.GetFiles("O:/NutrisensTest/Claims/Cargas");

            //Copy File names to Model collection.
            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {
                files.Add(new FileModel { FileName = Path.GetFileName(filePath) });
            }

            return files;
        }

        private DataTable ListaCargas()
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "LISTA-CARGAS" },
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS_CARGAS", CommandType.StoredProcedure, parameters);

            return dt;
        }

        public IActionResult ObtenerListaCargas()
        {
            return PartialView("UploadsGrid", ListaCargas());
        }

        public FileResult DownloadFile(string fileName, string empresa)
        {
            //Build the File Path.
            string path = Path.Combine("O:/NutrisensTest/Claims/Cargas/") + empresa + "/" + fileName;

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        private DataTable ListaClaims(string empresa, string estado, string fecha_inicio, string fecha_fin, string estadoAE, string estadoAI)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SELECT" },
             new SqlParameter("@empresa", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = empresa },
             new SqlParameter("@estado", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = estado },
             new SqlParameter("@fecha_inicio", SqlDbType.Date) { Direction = ParameterDirection.Input, Value = fecha_inicio },
             new SqlParameter("@fecha_fin", SqlDbType.Date) { Direction = ParameterDirection.Input, Value = fecha_fin },
             new SqlParameter("@estadoAE", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = estadoAE },
             new SqlParameter("@estadoAI", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = estadoAI }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaAETodas(string empresa, string estadoAE, string responsable)
        {
            var parameters = new[] {
             new SqlParameter("@empresa", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = empresa },
             new SqlParameter("@estadoAE", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = estadoAE },
             new SqlParameter("@responsable", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = responsable }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("LISTA_CLAIMS_AE", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaAITodas(string empresa, string estadoAI, string responsable)
        {
            var parameters = new[] {
             new SqlParameter("@empresa", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = empresa },
             new SqlParameter("@estadoAI", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = estadoAI },
             new SqlParameter("@responsable", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = responsable }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("LISTA_CLAIMS_AI", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaLotes(string codigo)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SELECT" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS_LOTES", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaNotas(string codigo)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SELECT" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS_NOTAS", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaAE(string codigo)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SELECT-AE" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaNotasAE(string codigoAE)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SELECT-NOTAS" },
             new SqlParameter("@codigoAE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAE }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaAI(string codigo)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SELECT-AI" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);

            return dt;
        }

        private DataTable ListaNotasAI(string codigoAI)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "SELECT-NOTAS" },
             new SqlParameter("@codigoAI", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAI }
             };

            DataTable dt = accesoDatos.EjecutarProcedimientoDatatable("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);

            return dt;
        }

        public IActionResult ObtenerClaims(string empresa, string estado, string fecha_inicio, string fecha_fin, string estadoAE, string estadoAI)
        {
            return PartialView("ClaimsGrid", ListaClaims(empresa, estado, fecha_inicio, fecha_fin, estadoAE, estadoAI));
        }

        public IActionResult ObtenerTodasAE(string empresa, string estadoAE, string responsable)
        {
            return PartialView("AETodasGrid", ListaAETodas(empresa, estadoAE, responsable));
        }

        public IActionResult ObtenerTodasAI(string empresa, string estadoAI, string responsable)
        {
            return PartialView("AITodasGrid", ListaAITodas(empresa, estadoAI, responsable));
        }

        public IActionResult ObtenerClientes(string empresa)
        {
            return PartialView("ClientesGrid", ListaClientes(empresa));
        }

        public IActionResult ObtenerReferencias(string empresa)
        {
            return PartialView("ReferenciasGrid", ListaReferencias(empresa));
        }

        public IActionResult ObtenerLotes(string codigo, string nivel)
        {
            ViewData["nivel"] = nivel;

            return PartialView("LotesGrid", ListaLotes(codigo));
        }

        public IActionResult ObtenerNotas(string codigo, string nivel)
        {
            ViewData["nivel"] = nivel;

            return PartialView("NotasGrid", ListaNotas(codigo));
        }

        public IActionResult ObtenerNotasAE(string codigoAE, string nivel)
        {
            ViewData["nivel"] = nivel;

            return PartialView("NotasAEGrid", ListaNotasAE(codigoAE));
        }

        public IActionResult ObtenerNotasAI(string codigoAI, string nivel)
        {
            ViewData["nivel"] = nivel;

            return PartialView("NotasAIGrid", ListaNotasAI(codigoAI));
        }

        public IActionResult ObtenerListaAE(string codigo, string nivel, string perfil)
        {
            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;

            return PartialView("AEGrid", ListaAE(codigo));
        }

        public IActionResult ObtenerListaAI(string codigo, string nivel, string perfil)
        {
            ViewData["nivel"] = nivel;
            ViewData["perfil"] = perfil;

            return PartialView("AIGrid", ListaAI(codigo));
        }

        private Claim GuardarClaim(Claim modelo)
        {
            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "GUARDAR" },
             new SqlParameter("@empresa", SqlDbType.Char) { Direction = ParameterDirection.Input, Value = modelo.Empresa },
             new SqlParameter("@codigo_cliente", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = modelo.CodigoCliente },
             new SqlParameter("@num_pedido", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = modelo.NumPedido },
             new SqlParameter("@motivo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = modelo.Motivo },
             new SqlParameter("@fecha_alta", SqlDbType.Date) { Direction = ParameterDirection.Input, Value = modelo.FechaAlta },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = modelo.Codigo }

             };

            SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_CLAIMS", CommandType.StoredProcedure, parameters);

            while (reader.Read())
            {
                if (string.IsNullOrEmpty(modelo.Codigo))
                {
                    modelo.Codigo = reader["codigo_creado"].ToString();
                }
            }

            reader.Close();

            return modelo;

        }

        private Claim ObtenerClaim(string codigo)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-CLAIM" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo }
             };

            SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_CLAIMS", CommandType.StoredProcedure, parameters);

            Claim modelo = new Claim();

            while (reader.Read())
            {
                if (int.TryParse(reader["Empresa"].ToString(), out int empresa))
                {
                    modelo.Empresa = empresa;
                }

                modelo.Codigo = reader["Codigo"].ToString();

                if (int.TryParse(reader["Estado"].ToString(), out int estado))
                {
                    modelo.Estado = estado;
                }
                else
                {
                    modelo.Estado = 0;
                }

                modelo.EstadoNombre = reader["EstadoNombre"].ToString();
                modelo.CodigoCliente = reader["CodigoCliente"].ToString();
                modelo.NombreCliente = reader["NombreCliente"].ToString();
                modelo.NumPedido = reader["NumPedido"].ToString();
                modelo.Motivo = reader["Motivo"].ToString();
                modelo.UsuarioAlta = reader["UsuarioAlta"].ToString();
                modelo.UsuarioAltaNombre = reader["UsuarioAltaNombre"].ToString();
                modelo.UsuarioMod = reader["UsuarioModNombre"].ToString();

                if (DateTime.TryParse(reader["FechaAltaCorta"].ToString(), out DateTime fecha_alta))
                {
                    modelo.FechaAlta = fecha_alta;
                }

                if (DateTime.TryParse(reader["FechaMod"].ToString(), out DateTime fecha_mod))
                {
                    modelo.FechaMod = fecha_mod;
                }

                modelo.EstadoAE = reader["EstadoAE"].ToString();
                modelo.EstadoAI = reader["EstadoAI"].ToString();

            }
            reader.Close();

            return modelo;
        }


        public IActionResult GuardarLote(string codigo, string codigo_ref, string numero_lote, string cantidad, string unidad)
        {

            string mensaje = "";

            try
            {
                var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "GUARDAR" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@codigo_ref", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo_ref },
             new SqlParameter("@numero_lote", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = numero_lote },
             new SqlParameter("@cantidad", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = cantidad },
             new SqlParameter("@unidad", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = unidad }

             };

                accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_LOTES", CommandType.StoredProcedure, parameters);


                mensaje = "Batch added succesfully";
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Content(mensaje);

        }

        public void EliminarLote(string codigo, string codigo_ref, string numero_lote)
        {
            var parameters = new[] {
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "ELIMINAR" },
             new SqlParameter("@codigo_ref", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo_ref },
             new SqlParameter("@numero_lote", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = numero_lote }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_LOTES", CommandType.StoredProcedure, parameters);

        }

        public IActionResult GuardarNota(string codigo, string nota)
        {
            string mensaje = "";

            try
            {
                var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "GUARDAR" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@nota", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nota }
             };

                accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_NOTAS", CommandType.StoredProcedure, parameters);


                mensaje = "Note added succesfully";
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Content(mensaje);

        }

        public void EliminarNota(string id)
        {
            var parameters = new[] {
             new SqlParameter("@id", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = id },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "ELIMINAR" }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_NOTAS", CommandType.StoredProcedure, parameters);

        }

        [HttpPost]
        public IActionResult GuardarAE(string codigo, string accion, string responsable, string fecha_limite, string codigoAE, string[] usuarios_informar)
        {
            string mensaje = "", codigo_AE = "";

            try
            {
                var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "GUARDAR-AE" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@responsable", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = responsable },
             new SqlParameter("@accion", SqlDbType.SmallInt) { Direction = ParameterDirection.Input, Value = accion },
             new SqlParameter("@fecha_limite", SqlDbType.Date) { Direction = ParameterDirection.Input, Value = fecha_limite },
             new SqlParameter("@codigoAE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAE }

             };

                SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);

                while (reader.Read())
                {
                    codigo_AE = reader["CodigoAE"].ToString();
                    mensaje = reader["mensaje"].ToString();
                }

                reader.Close();

                if (usuarios_informar != null)
                {
                    EmailSender Mail = new EmailSender();

                    for (int i = 0; i < usuarios_informar.Length; i++)
                    {
                        Mail.Enviar(usuarios_informar[i], "New External Action", "The EA number " + codigo_AE + " has been registered. " +
                            "<p><a href='http://dev.vegenat.net/nutrisenstest/Claims/ClaimsDetail?codigo=" + codigo + "' target='_blank'>Click here</a></p>");
                    }
                }

            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Json(new { mensaje = mensaje, codigo_AE = codigo_AE });

        }

        [HttpPost]
        public IActionResult GuardarAI(string codigo, string analisis_causa, string accion_correctiva, string responsable
                        , string fecha_limite, string codigoAI, string[] usuarios_informar)
        {
            string mensaje = "", codigo_AI = "";

            try
            {
                var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "GUARDAR-AI" },
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@responsable", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = responsable },
             new SqlParameter("@analisis_causa", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = analisis_causa },
             new SqlParameter("@accion_correctiva", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = accion_correctiva },
             new SqlParameter("@fecha_limite", SqlDbType.Date) { Direction = ParameterDirection.Input, Value = fecha_limite },
             new SqlParameter("@codigoAI", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAI }

             };

                SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);

                while (reader.Read())
                {
                    codigo_AI = reader["CodigoAI"].ToString();
                    mensaje = reader["mensaje"].ToString();
                }

                reader.Close();

                if (usuarios_informar != null)
                {
                    EmailSender Mail = new EmailSender();

                    for (int i = 0; i < usuarios_informar.Length; i++)
                    {
                        Mail.Enviar(usuarios_informar[i], "New Internal Action", "The IA number " + codigo_AI + " has been registered. " +
                            "<p><a href='http://dev.vegenat.net/nutrisenstest/Claims/ClaimsDetail?codigo=" + codigo + "' target='_blank'>Click here</a></p>");
                    }
                }

            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Json(new { mensaje = mensaje, codigo_AI = codigo_AI });

        }

        public void EliminarAE(string codigoAE)
        {
            var parameters = new[] {
             new SqlParameter("@codigoAE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAE },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "ELIMINAR-AE" }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);

        }

        public void EliminarAI(string codigoAI)
        {
            var parameters = new[] {
             new SqlParameter("@codigoAI", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAI },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "ELIMINAR-AI" }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);

        }

        public IActionResult ObtenerAE(string codigoAE)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-AE" },
             new SqlParameter("@codigoAE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAE }
             };

            Dictionary<string, string> dict = new Dictionary<string, string>();

            SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);

            while (reader.Read())
            {
                dict["CodigoAE"] = reader["CodigoAE"].ToString();
                dict["Accion"] = reader["Accion"].ToString();
                dict["EstadoAE"] = reader["EstadoAE"].ToString();
                dict["NombreEstado"] = reader["NombreEstado"].ToString();
                dict["Responsable"] = reader["Responsable"].ToString();
                dict["FechaLimite"] = reader["FechaLimite"].ToString();
            }

            reader.Close();

            return Json(dict);
        }

        public IActionResult ObtenerAI(string codigoAI)
        {
            var parameters = new[] {
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-AI" },
             new SqlParameter("@codigoAI", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAI }
             };

            Dictionary<string, string> dict = new Dictionary<string, string>();

            SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);

            while (reader.Read())
            {
                dict["CodigoAI"] = reader["CodigoAI"].ToString();
                dict["EstadoAI"] = reader["EstadoAI"].ToString();
                dict["NombreEstado"] = reader["NombreEstado"].ToString();
                dict["AnalisisCausa"] = reader["AnalisisCausa"].ToString();
                dict["AccionCorrectiva"] = reader["AccionCorrectiva"].ToString();
                dict["Responsable"] = reader["Responsable"].ToString();
                dict["FechaLimite"] = reader["FechaLimite"].ToString();
            }

            reader.Close();

            return Json(dict);
        }

        public IActionResult GuardarNotaAE(string codigo, string nota)
        {
            string mensaje = "";

            try
            {
                var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "GUARDAR-NOTA" },
             new SqlParameter("@codigoAE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@nota", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nota }
             };

                accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);


                mensaje = "Note added succesfully";
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Content(mensaje);

        }

        public IActionResult GuardarNotaAI(string codigo, string nota)
        {
            string mensaje = "";

            try
            {
                var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "GUARDAR-NOTA" },
             new SqlParameter("@codigoAI", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@nota", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = nota }
             };

                accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);


                mensaje = "Note added succesfully";
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Content(mensaje);

        }

        public void EliminarNotaAE(string id)
        {
            var parameters = new[] {
             new SqlParameter("@id_nota", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "ELIMINAR-NOTA" }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);

        }

        public void EliminarNotaAI(string id)
        {
            var parameters = new[] {
             new SqlParameter("@id_nota", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = id },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "ELIMINAR-NOTA" }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);

        }

        public void CerrarAE(string codigoAE)
        {
            var parameters = new[] {
             new SqlParameter("@codigoAE", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAE },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "CERRAR-AE" }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AE", CommandType.StoredProcedure, parameters);

        }

        public void CerrarAI(string codigoAI)
        {
            var parameters = new[] {
             new SqlParameter("@codigoAI", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigoAI },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "CERRAR-AI" }
             };

            accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS_AI", CommandType.StoredProcedure, parameters);

        }

        public IActionResult InformarUsuarios(string codigo, string codigo_AE, string[] usuarios_informar)
        {
            string mensaje = "";

            try
            {
                if (usuarios_informar != null)
                {
                    EmailSender Mail = new EmailSender();

                    for (int i = 0; i < usuarios_informar.Length; i++)
                    {
                        Mail.Enviar(usuarios_informar[i], "External Action Closed", "The EA number " + codigo_AE + " has been closed. " +
                            "<p><a href='http://dev.vegenat.net/nutrisenstest/Claims/ClaimsDetail?codigo=" + codigo + "' target='_blank'>Click here</a></p>");
                    }

                    mensaje = "E-mail sent succesfully";
                }

            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Content(mensaje);

        }

        public IActionResult InformarUsuariosAI(string codigo, string codigo_AI, string[] usuarios_informar)
        {
            string mensaje = "";

            try
            {
                if (usuarios_informar != null)
                {
                    EmailSender Mail = new EmailSender();

                    for (int i = 0; i < usuarios_informar.Length; i++)
                    {
                        Mail.Enviar(usuarios_informar[i], "Internal Action Closed", "The IA number " + codigo_AI + " has been closed. " +
                            "<p><a href='http://dev.vegenat.net/nutrisenstest/Claims/ClaimsDetail?codigo=" + codigo + "' target='_blank'>Click here</a></p>");
                    }

                    mensaje = "E-mail sent succesfully";
                }

            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Content(mensaje);

        }

        public IActionResult CerrarClaim(string codigo, string fecha_cierre)
        {
            string mensaje = "";

            try
            {
                var parameters = new[] {
             new SqlParameter("@codigo", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = codigo },
             new SqlParameter("@fecha_cierre", SqlDbType.Date) { Direction = ParameterDirection.Input, Value = fecha_cierre },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "CERRAR-CLAIM" },
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value =  User.Identity.Name }
             };

                accesoDatos.EjecutarProcedimiento("CRUD_CLAIMS", CommandType.StoredProcedure, parameters);

                mensaje = "Claim closed succesfully";
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }

            return Content(mensaje);

        }
    }
}






// Función para convertir un Excel en objeto Datatable
public static class ExcelPackageExtensions
{
    public static DataTable ToDataTable(this ExcelPackage package)
    {
        ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
        DataTable table = new DataTable();
        foreach (var firstRowCell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
        {
            table.Columns.Add(firstRowCell.Text);
        }
        for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
        {
            var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
            var newRow = table.NewRow();
            foreach (var cell in row)
            {
                newRow[cell.Start.Column - 1] = cell.Text;
            }
            table.Rows.Add(newRow);
        }
        return table;
    }
}