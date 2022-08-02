namespace Presupuestos.Models
{
    public class CuentaContable
    {
        public Int64 ID { get; set; }
        public string NUMERO { get; set; }
        public Int64 IDENTI_CUENTA_GERAPLI { get; set; }

        public string NOMBRE { get; set; }

        public string CLASIFICACION { get; set; }

        public DateTime fh_mod { get; set; }

        public Int16 borrado { get; set; }
    }
}
