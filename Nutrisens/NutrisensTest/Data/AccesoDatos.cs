using Microsoft.Data.SqlClient;
using System.Data;

namespace Nutrisens.Data
{
    public class AccesoDatos
    {
        private static string strConexion;


        public AccesoDatos()
        {
            //var builder = WebApplication.CreateBuilder();

            strConexion = "Data Source=AURABBDD\\MSSQLSERVER2012;Initial Catalog=GRUPO_NUTRISENS;User ID=sa; Password=Vegenat2011; Connection Timeout=300;";
        }

        private static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(strConexion);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }


        public void EjecutarProcedimiento(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.CommandType = commandType;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.AddRange(parameters);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                }

            }
            catch (Exception ex)
            {
                saveLogError(ex.Message);
                throw;

            }

        }

        public SqlCommand EjecutarProcedimientoConParametros(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            SqlCommand cmd;

            using (SqlConnection conn = GetConnection())
            {
                cmd = new SqlCommand(commandText, conn);

                try
                {
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    saveLogError(ex.Message);

                    throw;

                }

            }

            return cmd;
        }

        public DataTable EjecutarProcedimientoDatatable(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.CommandType = commandType;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.AddRange(parameters);
                        SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        dt.Load(dr);

                    }
                }
            }
            catch (Exception ex)
            {
                saveLogError(ex.Message);

                throw;

            }
            return dt;


        }

        public DataSet GetDataSet(string storedProcName, List<SqlParameter> parameters)
        {
            var result = new DataSet();

            try
            {

                using (SqlConnection connection = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcName, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (parameters != null && parameters.Count > 0)
                        {

                            cmd.Parameters.AddRange(parameters.ToArray());
                        }

                        var dataAdapter = new SqlDataAdapter(cmd);
                        dataAdapter.Fill(result);
                        saveLog(storedProcName, parameters, connection);
                    }
                }


            }
            catch (Exception ex)
            {
                saveLogError(ex.Message);

                throw;

            }




            return result;
        }


        public SqlDataReader EjecutarReader(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection(strConexion);

            using (SqlCommand cmd = new SqlCommand(commandText, conn))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = 600;
                cmd.Parameters.AddRange(parameters);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return dr;
            }
        }


        // Método para obtener el perfil que tiene el usuario en la aplicación
        public int GetPerfil(string login, int aplicacion)
        {
            int perfil = 0;

            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login },
             new SqlParameter("@aplicacion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = aplicacion },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-PERFIL" }
             };

            SqlDataReader reader = EjecutarReader("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            while (reader.Read())
            {
                perfil = Convert.ToInt32(reader["perfil"].ToString());
            }
            reader.Close();

            return perfil;
        }

        // Método para obtener el nivel de acceso que tiene el usuario en la sección
        public int GetNivel(string login, int seccion)
        {
            int nivel = 0;

            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login },
             new SqlParameter("@seccion", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = seccion },
             new SqlParameter("@operacion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = "OBTENER-NIVEL" }
             };

            SqlDataReader reader = EjecutarReader("CRUD_USUARIOS", CommandType.StoredProcedure, parameters);

            while (reader.Read())
            {
                nivel = Convert.ToInt32(reader["nivel"].ToString());
            }
            reader.Close();

            return nivel;
        }

        public void saveLog(string procedimiento, List<SqlParameter> parameters, SqlConnection connection)
        {
            string parametros = "Parámetros: { ";

            if (parameters != null)
            {
                parameters.ForEach(delegate (SqlParameter thisParam)
                {
                    parametros += thisParam.ParameterName + " => " + thisParam.Value + " , ";
                });
            }


            parametros += "}";
            string text = DateTime.Now.ToString() + ":" + procedimiento + ":" + parametros;
            string procedureName = "CRUD_LOG";

            using (SqlCommand cmd = new SqlCommand(procedureName, connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@texto";
                parameter.Value = text;

                cmd.Parameters.Add(parameter);

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

        }

        public void saveLogError(string exception)
        {

            using (SqlConnection connection = GetConnection())
            {
                string excepcion = "EXCEPTION: " + exception;

                string text = DateTime.Now.ToString() + ":" + excepcion;
                string procedureName = "CRUD_LOG";


                using (SqlCommand cmd = new SqlCommand(procedureName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "@texto";
                    parameter.Value = text;

                    cmd.Parameters.Add(parameter);

                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }


            }


        }

    }



}
