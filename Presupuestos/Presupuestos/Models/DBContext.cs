using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Presupuestos.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
         : base(options)
        {
        }
    }
}
