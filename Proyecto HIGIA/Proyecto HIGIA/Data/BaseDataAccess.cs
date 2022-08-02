using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Proyecto_HIGIA.Data
{

    public class BaseDataAccess
    {
        private static string ConnectionString { get; set; }


        public BaseDataAccess()
        {
            ConnectionString = "Data Source=AURABBDD\\MSSQLSERVER2012;Initial Catalog=HIGIA;User ID=sa; Password=Vegenat2011; Connection Timeout=3000;";
        }

        private static SqlConnection GetConnection()
        {
            ConnectionString = "Data Source=AURABBDD\\MSSQLSERVER2012;Initial Catalog=HIGIA;User ID=sa; Password=Vegenat2011; Connection Timeout=3000;";

            SqlConnection connection = new SqlConnection(ConnectionString);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }

        private static SqlCommand GetCommand(DbConnection connection, string commandText, CommandType commandType)
        {
            SqlCommand command = new SqlCommand(commandText, connection as SqlConnection);
            command.CommandType = commandType;
            return command;
        }

        protected SqlParameter GetParameter(string parameter, object value)
        {
            SqlParameter parameterObject = new SqlParameter(parameter, value != null ? value : DBNull.Value);
            parameterObject.Direction = ParameterDirection.Input;
            return parameterObject;
        }

        protected SqlParameter GetParameterOut(string parameter, SqlDbType type, object value = null, ParameterDirection parameterDirection = ParameterDirection.InputOutput)
        {
            SqlParameter parameterObject = new SqlParameter(parameter, type); ;

            if (type == SqlDbType.NVarChar || type == SqlDbType.VarChar || type == SqlDbType.NText || type == SqlDbType.Text)
            {
                parameterObject.Size = -1;
            }

            parameterObject.Direction = parameterDirection;

            if (value != null)
            {
                parameterObject.Value = value;
            }
            else
            {
                parameterObject.Value = DBNull.Value;
            }

            return parameterObject;
        }

        public static int ExecuteNonQuery(string procedureName, List<SqlParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            int returnValue = -1;

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    DbCommand cmd = GetCommand(connection, procedureName, commandType);

                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    returnValue = cmd.ExecuteNonQuery();
                    saveLog(procedureName, parameters, connection);
                }
            }
            catch (Exception ex)
            {
                saveLogError(ex.Message);

                //LogException("Failed to ExecuteNonQuery for " + procedureName, ex, parameters);
                throw;
            }

            return returnValue;
        }

        public static SqlCommand ExecuteScalar(string procedureName, List<SqlParameter> parameters)
        {
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    cmd = GetCommand(connection, procedureName, CommandType.StoredProcedure);

                    if (parameters != null && parameters.Count > 0)
                    {

                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    int i = cmd.ExecuteNonQuery();
                    saveLog(procedureName, parameters, connection);
                    //Storing the output parameters value in 3 different variables.  
                }
            }
            catch (Exception ex)
            {
                saveLogError(ex.Message);

                //LogException("Failed to ExecuteScalar for " + procedureName, ex, parameters);
                throw;
            }

            return cmd;
        }

        public static SqlDataReader GetDataReader(string procedureName, List<SqlParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            SqlDataReader ds;

            try
            {
                SqlConnection connection = GetConnection();
                {
                    SqlCommand cmd = GetCommand(connection, procedureName, commandType);
                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    saveLog(procedureName, parameters, connection);

                    ds = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (Exception ex)
            {
                saveLogError(ex.Message);

                //LogException("Failed to GetDataReader for " + procedureName, ex, parameters);
                throw;
            }

            return ds;
        }

        public static DataSet GetDataSet(string storedProcName, List<SqlParameter> parameters)
        {
            var result = new DataSet();


            using (SqlConnection connection = GetConnection())
            {
                SqlCommand cmd = GetCommand(connection, storedProcName, CommandType.StoredProcedure);

                if (parameters != null && parameters.Count > 0)
                {

                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                var dataAdapter = new SqlDataAdapter(cmd);
                dataAdapter.Fill(result);
            }
      

            return result;
        }


        public static void saveLog(string procedimiento, List<SqlParameter> parameters, SqlConnection connection)
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
            string procedureName = "SP_SAVE_LOG";

            DbCommand cmd = GetCommand(connection, procedureName, CommandType.StoredProcedure);

            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@text";
            parameter.Value = text;

            cmd.Parameters.Add(parameter);

            int returnValue = cmd.ExecuteNonQuery();

        }

        public static void saveLogError(string exception)
        {
            using (SqlConnection connection = GetConnection())
            {
                string excepcion = "EXCEPTION: " + exception;

                string text = DateTime.Now.ToString() + ":" + excepcion;
                string procedureName = "SP_SAVE_LOG";

                DbCommand cmd = GetCommand(connection, procedureName, CommandType.StoredProcedure);

                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@text";
                parameter.Value = text;

                cmd.Parameters.Add(parameter);

                int returnValue = cmd.ExecuteNonQuery();


            }


        }
    }
}

