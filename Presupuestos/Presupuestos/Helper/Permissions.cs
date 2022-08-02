
using Presupuestos.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Presupuestos.Helper
{
    public class Permissions
    {
        private static DBContext dbContext;
        private string strConexion;

        public Permissions(DBContext usrctxt, IConfiguration configuration)
        {
            dbContext = usrctxt;
            strConexion = configuration.GetConnectionString("SqlServer");
        }

        public static int ComprobarPermiso(string login, int seccion)
        {
            int level = 0;

            var parameters = new[] {
             new SqlParameter("@usuario", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = login },
             new SqlParameter("@seccion", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = seccion },
             new SqlParameter("@nivel", SqlDbType.Int) { Direction = ParameterDirection.Output }
             };

            dbContext.Database.ExecuteSqlRaw("SP_COMPROBAR_PERMISO @usuario={0}, @seccion={1}, @nivel={2} Output", parameters);

            level = Convert.ToInt32(parameters[2].Value.ToString());

            return level;
        }




    }
}
