using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Nutrisens.Models;
using Nutrisens.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Nutrisens.Services;
using NutrisensTest.Models;

namespace NutrisensTest.Controllers
{
    [Authorize]
    public class PDPController : Controller
    {
        private int perfil = 0, nivel = 0, aplicacion, seccion;
        private string nombre_perfil = "", login;
        private AccesoDatos accesoDatos;
        public PDPController()
        {
            accesoDatos = new AccesoDatos();
        }
        public IActionResult Index()
        {
            aplicacion = 2;
            login = User.Identity.Name;
            perfil = GetPerfil(User.Identity.Name, aplicacion);
            ViewData["perfil"] = perfil;
            seccion = 6;

            if (perfil < 3)
            {
                nivel = 1;
            }
            else
            {
                nivel = GetNivel(login, seccion);
            }

            ViewData["nivel"] = nivel;

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                return View();
            }


        }

        /**
         * New Proposal
         */

        public IActionResult NewProposal(string number)
        {
            aplicacion = 2;

            var user = User;
            perfil = GetPerfil(User.Identity.Name, aplicacion);
            login = User.Identity.Name;
            ViewData["nombre_perfil"] = login;
            ViewData["perfil"] = perfil;
            seccion = 7;

            if (perfil < 3)
            {
                nivel = 1;
            }
            else
            {
                nivel = GetNivel(login, seccion);
            }



            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {

                Proposal pro = new Proposal();
                if (number != "" && number != null)
                {
                    var parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@proposal", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = number });


                    var dataSet = accesoDatos.GetDataSet("GET_PROPOSAL", parameters);

                    var dt_proposal = dataSet?.Tables?[0];
                    var proposal = dt_proposal.Rows[0];
                    pro.state = Convert.ToString(proposal["state"]);
                    pro.numero = Convert.ToString(proposal["number"]);
                    pro.business_unit = Convert.ToString(proposal["business_unit"]);
                    pro.creator = Convert.ToString(proposal["user_create"]);
                    pro.approver_1 = Convert.ToString(proposal["user_approver_1"]);
                    pro.approver_2 = Convert.ToString(proposal["user_approver_2"]);
                    pro.approver_3 = Convert.ToString(proposal["user_approver_3"]);
                }
                else
                {
                    pro.state = "Pending to send";
                }

                return View(pro);
            }


        }

        public IActionResult NuevoNumero()
        {
            var response = new JObject();

            var parameters = new[] {
                new SqlParameter("@numero", SqlDbType.Int) {Direction = ParameterDirection.Output}
             };

            SqlCommand command = accesoDatos.EjecutarProcedimientoConParametros("GET_NUEVO_NUMERO_PROPOSAL", CommandType.StoredProcedure, parameters);

            if (command.Parameters["@numero"].Value != null)
            {

                int numero = int.Parse(command.Parameters["@numero"].Value.ToString());
                //response["numero"] = "PDP - " + numero;
                response["numero"] = numero;
            }

            return Content(response.ToString(), "application/json");
        }

        public IActionResult CRUDProposal(string operacion, int company, string number, string business_unit, string user_approver_1, string user_approver_2, string user_approver_3, string project_name, string datatables, string information, string unit_macronutrientes, string unit_minerals, string unit_vitamins, string send = "false")
        {
            var response = new JObject();

      

            var parameters = new[] {
                new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = operacion },
                new SqlParameter("@company", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = company },
                new SqlParameter("@number", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = number },
                new SqlParameter("@business_unit", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = business_unit },
                new SqlParameter("@creator", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
                new SqlParameter("@approver_1", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = user_approver_1 },
                new SqlParameter("@approver_2", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = user_approver_2 },
                new SqlParameter("@approver_3", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = user_approver_3 },
                new SqlParameter("@project_name", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = project_name },
                new SqlParameter("@usu_mod", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
                new SqlParameter("@unit_macronutrientes", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = unit_macronutrientes },
                new SqlParameter("@unit_minerals", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = unit_minerals },
                new SqlParameter("@unit_vitamins", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = unit_vitamins },
                new SqlParameter("@mensaje", SqlDbType.VarChar, 200) {Direction = ParameterDirection.Output},
                new SqlParameter("@success", SqlDbType.Int) {Direction = ParameterDirection.Output},
                new SqlParameter("@identificador", SqlDbType.Int) {Direction = ParameterDirection.Output}
            };

            SqlCommand command = accesoDatos.EjecutarProcedimientoConParametros("CRUD_PROPOSALS", CommandType.StoredProcedure, parameters);

            if (command.Parameters["@success"].Value != null)
            {

                response["success"] = int.Parse(command.Parameters["@success"].Value.ToString());
                response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();
                int identificador = int.Parse(command.Parameters["@identificador"].Value.ToString());

                if (operacion == "INSERT")
                {
                    saveNutritionalInformation(identificador, datatables);
                    saveGeneralInformation(information, identificador);
                }

                if (send == "true")
                {

                    parameters = new[] {
                        new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = "UPDATE_STATE" },
                        new SqlParameter("@state", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = "Pending Approval 1" },
                        new SqlParameter("@number", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = number },
                        new SqlParameter("@usu_mod", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
                        new SqlParameter("@mensaje", SqlDbType.VarChar, 200) {Direction = ParameterDirection.Output},
                        new SqlParameter("@success", SqlDbType.Int) {Direction = ParameterDirection.Output},
                        new SqlParameter("@identificador", SqlDbType.Int) {Direction = ParameterDirection.Output}
                    };

                    command = accesoDatos.EjecutarProcedimientoConParametros("CRUD_PROPOSALS", CommandType.StoredProcedure, parameters);
                    var text = getMailText("Pending Approval 1", number);
                    EmailSender e = new EmailSender();
                    e.Enviar(user_approver_1, "Proposal pending appproval", text);
                }

            }

            return Content(response.ToString(), "application/json");
        }

        public void saveNutritionalInformation(int identificador, string datatables)
        {
            JObject obj_datatables = JObject.Parse(datatables);

            JArray dt_macronutrientes = (JArray)obj_datatables["macronutrientes"];

            foreach (JObject item in dt_macronutrientes)
            {
                object valor = null;
                if (item.GetValue("valor") != null && item.GetValue("valor").ToString() != "")
                {
                    valor = Double.Parse(item.GetValue("valor").ToString());
                }


                var parameters = new[] {
                 new SqlParameter("@proposal", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = identificador },
                 new SqlParameter("@tabla", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "Macronutrientes" },
                 new SqlParameter("@nombre", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = item.GetValue("macronutriente").ToString() },
                 new SqlParameter("@unidad", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = item.GetValue("unidad").ToString() },
                 new SqlParameter("@valor", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = valor }
             };

                accesoDatos.EjecutarProcedimiento("SP_GUARDAR_VALOR_NUTRICIONAL", CommandType.StoredProcedure, parameters);
            }

            JArray dt_minerales = (JArray)obj_datatables["minerales"];

            foreach (JObject item in dt_minerales)
            {
                object valor = null;
                if (item.GetValue("valor") != null && item.GetValue("valor").ToString() != "")
                {
                    valor = Double.Parse(item.GetValue("valor").ToString());
                }

                var parameters = new[] {
                 new SqlParameter("@proposal", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = identificador },
                 new SqlParameter("@tabla", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "Minerales" },
                 new SqlParameter("@nombre", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = item.GetValue("mineral").ToString() },
                 new SqlParameter("@unidad", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = item.GetValue("unidad").ToString() },
                 new SqlParameter("@valor", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = valor }
             };

                accesoDatos.EjecutarProcedimiento("SP_GUARDAR_VALOR_NUTRICIONAL", CommandType.StoredProcedure, parameters);
            }

            JArray dt_vitaminas = (JArray)obj_datatables["vitaminas"];

            foreach (JObject item in dt_vitaminas)
            {
                object valor = null;
                if (item.GetValue("valor") != null && item.GetValue("valor").ToString() != "")
                {
                    valor = Double.Parse(item.GetValue("valor").ToString());
                }

                var parameters = new[] {
                 new SqlParameter("@proposal", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = identificador },
                 new SqlParameter("@tabla", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "Vitaminas" },
                 new SqlParameter("@nombre", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = item.GetValue("vitamina").ToString() },
                 new SqlParameter("@unidad", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = item.GetValue("unidad").ToString() },
                 new SqlParameter("@valor", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = valor }
             };

                accesoDatos.EjecutarProcedimiento("SP_GUARDAR_VALOR_NUTRICIONAL", CommandType.StoredProcedure, parameters);
            }


        }

        public void saveGeneralInformation(string information, int identificador)
        {
            JObject obj_information = JObject.Parse(information);

            var parameters = new[] {
                new SqlParameter("@proposal", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = identificador },
                new SqlParameter("@planificacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = obj_information["planification"] },
                new SqlParameter("@dt_released_date", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = obj_information["released_date"] },
                new SqlParameter("@descripcion", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["product_description"] },
                new SqlParameter("@patent", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["patent_proposal"] },
                new SqlParameter("@regulations", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["regulations"] },
                new SqlParameter("@flavourings", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["flavourings"] },
                new SqlParameter("@nutricional_info", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["nutritional_info"] },
                new SqlParameter("@ingredientes_especificos", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["specific_needed"] },
                new SqlParameter("@requerimientos_quimicos", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["physical_requirements"] },
                new SqlParameter("@coste_objetivo", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["target_cost"] },
                new SqlParameter("@instrucciones", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["instructions_use"] },
                new SqlParameter("@parametros_salida", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["releasing_params"] },
                new SqlParameter("@paquete_primario", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["primary_packaging"] },
                new SqlParameter("@paquete_secundario", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["secondary_packaging"] },
                new SqlParameter("@volumen_anyo", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["volume_per_year"] },
                new SqlParameter("@fecha_caducidad", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["sell_by_year"] },
                new SqlParameter("@happcc_system", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = obj_information["happpcc_system"] }
            };

            accesoDatos.EjecutarProcedimiento("GUARDAR_INFO_PROPOSAL", CommandType.StoredProcedure, parameters);

        }

        public IActionResult getListaEmpresas()
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            var dataSet = accesoDatos.GetDataSet("LISTA_EMPRESAS", parameters);

            var dt_empresas = dataSet?.Tables?[0];
            response["empresas"] = JsonConvert.SerializeObject(dt_empresas);

            return Content(response.ToString(), "application/json");
        }

        public IActionResult getListaApprovers()
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();

            var dataSet = accesoDatos.GetDataSet("LISTA_APROBADORES", parameters);

            var dt_approvers = dataSet?.Tables?[0];
            response["approvers"] = JsonConvert.SerializeObject(dt_approvers);

            return Content(response.ToString(), "application/json");
        }

        public IActionResult getProposal(string proposal)
        {
            var response = new JObject();

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@proposal", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = proposal });


            var dataSet = accesoDatos.GetDataSet("GET_PROPOSAL", parameters);

            var dt_proposal = dataSet?.Tables?[0];
            response["proposal"] = JsonConvert.SerializeObject(dt_proposal);

            var dt_proposal_info = dataSet?.Tables?[1];
            response["proposal_info"] = JsonConvert.SerializeObject(dt_proposal_info);

            var dt_macronutrientes = dataSet?.Tables?[2];
            response["macronutrientes"] = JsonConvert.SerializeObject(dt_macronutrientes);

            var dt_minerals = dataSet?.Tables?[3];
            response["minerals"] = JsonConvert.SerializeObject(dt_minerals);

            var dt_vitamins = dataSet?.Tables?[4];
            response["vitamins"] = JsonConvert.SerializeObject(dt_vitamins);

            var dt_comments = dataSet?.Tables?[5];
            response["comments"] = JsonConvert.SerializeObject(dt_comments);

            var archivos = dataSet?.Tables?[6];
            response["documents"] = JsonConvert.SerializeObject(archivos);

            var dt_cycles = dataSet?.Tables?[8];
            response["cycle"] = JsonConvert.SerializeObject(dt_cycles);

            var dt_units = dataSet?.Tables?[7];

            if (dt_units.Rows.Count > 0)
            {
                response["unit_macronutrientes"] = Convert.ToString(dt_units.Rows[0]["m_unit"]);
                response["unit_minerales"] = Convert.ToString(dt_units.Rows[0]["min_unit"]);
                response["unit_vitaminas"] = Convert.ToString(dt_units.Rows[0]["v_unit"]);
            }
            else
            {
                response["unit"] = 0;
            }

            return Content(response.ToString(), "application/json");
        }

        public IActionResult cambiarEstado(string number, string estado_actual, string comment = "", string rejected = "")
        {
            var response = new JObject();

            var user = User.Identity.Name;

            var perfil = GetPerfil(user, 2);

            if (perfil != 4)
            {
                var proximo_estado = proximoEstado(estado_actual, rejected);

                var parameters = new[] {
                new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = "UPDATE_STATE" },
                new SqlParameter("@state", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = proximo_estado },
                new SqlParameter("@number", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = number },
                new SqlParameter("@usu_mod", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
                new SqlParameter("@mensaje", SqlDbType.VarChar, 200) {Direction = ParameterDirection.Output},
                new SqlParameter("@success", SqlDbType.Int) {Direction = ParameterDirection.Output},
                new SqlParameter("@identificador", SqlDbType.Int) {Direction = ParameterDirection.Output}
            };

                SqlCommand command = accesoDatos.EjecutarProcedimientoConParametros("CRUD_PROPOSALS", CommandType.StoredProcedure, parameters);

                if (command.Parameters["@success"].Value != null)
                {
                    response["success"] = int.Parse(command.Parameters["@success"].Value.ToString());
                    response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();

                    var parames = new List<SqlParameter>();
                    parames.Add(new SqlParameter("@proposal", SqlDbType.VarChar) { Direction = ParameterDirection.Input, Value = number });

                    var dataSet = accesoDatos.GetDataSet("GET_PROPOSAL", parames);
                    var dt_proposal = dataSet?.Tables?[0];
                    var proposal = dt_proposal.Rows[0];

                    var subject = getSubjectText(proximo_estado);
                    var text = getMailText(proximo_estado, number, comment);
                    var destinatarios = getDestinatarios(proposal, estado_actual, rejected);
                    response["destinatarios"] = destinatarios;
                    EmailSender e = new EmailSender();
                    e.Enviar(destinatarios, subject, text);
                }
            }
            else
            {
                response["success"] = 0;
                response["mensaje"] = "You don´t have permissions to change the state.";
            }


            return Content(response.ToString(), "application/json");
        }


        /**
         * All Proposals        
         * */

        public IActionResult getAllProposals()
        {
            var response = new JObject();

            var user = User.Identity.Name;

            var perfil = GetPerfil(user, 2);


            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@usuario", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = user });

            var dataSet = accesoDatos.GetDataSet("GET_PROPOSALS", parameters);

            var dt_my_proposal = dataSet?.Tables?[0];
            response["my_proposal"] = JsonConvert.SerializeObject(dt_my_proposal);

            if (perfil != 4)
            {
                var dt_appoved_1 = dataSet?.Tables?[1];
                response["appoved_1"] = JsonConvert.SerializeObject(dt_appoved_1);
                var dt_all_proposals = dataSet?.Tables?[2];
                response["all_proposals"] = JsonConvert.SerializeObject(dt_all_proposals);
            }


            return Content(response.ToString(), "application/json");
        }

        /**
         * Comments
         */

        public IActionResult CRUDComments(string operacion, int is_comment_reject, string proposal, string comment, int identificador)
        {
            var response = new JObject();

            var parameters = new[] {
                new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = operacion },
                new SqlParameter("@comment", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = comment },
                new SqlParameter("@proposal", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = proposal },
                new SqlParameter("@is_comment_reject", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = is_comment_reject },
                new SqlParameter("@usuario", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = User.Identity.Name },
                new SqlParameter("@identificador", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = identificador },
                new SqlParameter("@mensaje", SqlDbType.VarChar, 200) {Direction = ParameterDirection.Output},
                new SqlParameter("@success", SqlDbType.Int) {Direction = ParameterDirection.Output}
            };

            SqlCommand command = accesoDatos.EjecutarProcedimientoConParametros("CRUD_COMMENTS", CommandType.StoredProcedure, parameters);

            if (command.Parameters["@success"].Value != null)
            {
                response["success"] = int.Parse(command.Parameters["@success"].Value.ToString());
                response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();
            }

            return Content(response.ToString(), "application/json");
        }

        /**
         * Documentos
         */

        public FileResult downloadFile(string name, string numero)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes("O:/Nutrisens/PDP/" + numero + "/" + name);
            string fileName = name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }




        /*private JArray listaArchivosProposal(string numero)
        {

            var directoryName = "/ArchivosAdjuntosPDP/" + numero;
            Directory.CreateDirectory(directoryName);


            DirectoryInfo di = new DirectoryInfo("/ArchivosAdjuntosPDP/" + numero);
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
        }*/

        [HttpPost]
        public IActionResult FileUpload(List<IFormFile> files, string numero)
        {
            var response = new JObject();

            long size = files.Sum(f => f.Length);
            var user = User.Identity.Name;
            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {

                    var parameters = new[] {
                        new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = "INSERT" },
                        new SqlParameter("@user_upload", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = user },
                        new SqlParameter("@name", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = formFile.FileName },
                        new SqlParameter("@proposal", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = numero },
                        new SqlParameter("@mensaje", SqlDbType.VarChar, 200) {Direction = ParameterDirection.Output},
                        new SqlParameter("@success", SqlDbType.Int) {Direction = ParameterDirection.Output}
                    };

                    SqlCommand command = accesoDatos.EjecutarProcedimientoConParametros("CRUD_DOCUMENTOS", CommandType.StoredProcedure, parameters);

                    if (command.Parameters["@success"].Value != null)
                    {
                        int success = int.Parse(command.Parameters["@success"].Value.ToString());
                        response["success"] = success;
                        response["mensaje"] = command.Parameters["@mensaje"].Value.ToString();
                        if (success == 1)
                        {
                            // full path to file in temp location
                            //var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
                            var filePath = "O:/Nutrisens/PDP/" + numero + "/" + formFile.FileName;
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            filePaths.Add(filePath);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                formFile.CopyToAsync(stream);
                            }
                        }
                       

                    }

                    



                }
            }


            return Content(response.ToString(), "application/json");
        }


        public IActionResult DeleteFile(string name, string numero)
        {
            var response = new JObject();

            var filePath = "O:/Nutrisens/PDP/" + numero + "/" + name;

            System.IO.File.Delete(filePath);

            response["success"] = true;
            response["mensaje"] = "Archivo borrado correctamente";

            var parameters = new[] {
                new SqlParameter("@operacion", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Input, Value = "DELETE" },
                new SqlParameter("@name", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = name },
                new SqlParameter("@proposal", SqlDbType.Text) { Direction = ParameterDirection.Input, Value = numero }
            };

            SqlCommand command = accesoDatos.EjecutarProcedimientoConParametros("CRUD_DOCUMENTOS", CommandType.StoredProcedure, parameters);

            return Content(response.ToString(), "application/json");
        }

        private string proximoEstado(string estado_actual, string rejected)
        {
            string nuevo_estado = "";
            if (rejected == "rejected")
            {
                nuevo_estado = "Rejected";
            }
            else
            {
                switch (estado_actual)
                {
                    case "Pending to Send":
                        nuevo_estado = "Pending Approval 1";
                        break;
                    case "Pending Approval 1":
                        nuevo_estado = "Pending Approval 2";
                        break;
                    case "Pending Approval 2":
                        nuevo_estado = "Pending Approval 3";
                        break;
                    case "Pending Approval 3":
                        nuevo_estado = "Approved";
                        break;
                    case "Rejected":
                        nuevo_estado = "Rejected";
                        break;
                }

            }

            return nuevo_estado;
        }

        private string getMailText(string state, string number, string comment = "")
        {
            string text = "";

            switch (state)
            {
                case "Pending Approval 1":
                case "Pending Approval 2":
                case "Pending Approval 3":
                    text = "The proposal " + number + " is pending your approval. Click <a href='http://dev.vegenat.net/Nutrisens/PDP/NewProposal?number=" + number + "'>here</a> to access.";
                    break;
                case "Approved":
                    text = "The proposal " + number + " has been approved. Click <a href='http://dev.vegenat.net/Nutrisens/PDP/NewProposal?number=" + number + "'>here</a> to access.";
                    break;
                case "Rejected":
                    text = "The proposal " + number + " has been rejected. " + comment + ". Click <a href='http://dev.vegenat.net/Nutrisens/PDP/NewProposal?number=" + number + "'>here</a> to access.";
                    break;
            }


            return text;
        }

        private string getSubjectText(string state)
        {
            string text = "";

            switch (state)
            {
                case "Pending Approval 1":
                case "Pending Approval 2":
                case "Pending Approval 3":
                    text = "Proposal pending appproval";
                    break;
                case "Approved":
                    text = "Proposal Approved";
                    break;
                case "Rejected":
                    text = "Proposal rejected";
                    break;
            }


            return text;
        }

        private string getDestinatarios(DataRow proposal, string estado_anterior, string rejected)
        {
            string destinatarios = "";

            var approver_1 = Convert.ToString(proposal["user_approver_1"]);
            var approver_2 = Convert.ToString(proposal["user_approver_2"]);
            var approver_3 = Convert.ToString(proposal["user_approver_3"]);
            var state = Convert.ToString(proposal["state"]);
            var creator = Convert.ToString(proposal["user_create"]);
            if (rejected == "rejected")
            {

                switch (estado_anterior)
                {
                    case "Pending Approval 1":
                        destinatarios = creator;
                        break;
                    case "Pending Approval 2":
                        destinatarios = approver_1 + ";" + creator;
                        break;
                    case "Pending Approval 3":
                        destinatarios = approver_1 + ";" + approver_2 + ";" + creator;
                        break;
                }

            }
            else
            {
                switch (state)
                {
                    case "Pending Approval 1":
                        destinatarios = approver_1;
                        break;
                    case "Pending Approval 2":
                        destinatarios = approver_2;
                        break;
                    case "Pending Approval 3":
                        destinatarios = approver_3;
                        break;
                    case "Approved":
                        destinatarios = approver_1 + ";" + approver_2 + ";" + creator;
                        break;
                    case "Rejected":
                        destinatarios = approver_1 + ";" + creator;
                        break;
                }
            }

            return destinatarios;
        }

        private int GetPerfil(string login, int aplicacion)
        {
            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login },
             new SqlParameter("@aplicacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = aplicacion },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-PERFIL" }
             };

            SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            while (reader.Read())
            {
                perfil = Convert.ToInt32(reader["perfil"].ToString());
                nombre_perfil = reader["NombrePerfil"].ToString();
            }
            reader.Close();

            return perfil;
        }

        private int GetNivel(string login, int seccion)
        {
            int nivel = 0;

            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login },
             new SqlParameter("@seccion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = seccion },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-NIVEL" }
             };

            SqlDataReader reader = accesoDatos.EjecutarReader("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            while (reader.Read())
            {
                nivel = Convert.ToInt32(reader["nivel"].ToString());
            }
            reader.Close();

            return nivel;
        }

    }
}
