namespace Presupuestos.Models
{
    public class CentroCoste
    {
        public Int64 ID { get; set; }
        public Int64 IDENTI { get; set; }
        public string CODIGO { get; set; }
        public Int64 EBITDA { get; set; }

        public string NOMBRE { get; set; }

        public string CLASIFICACION { get; set; }

        public DateTime fh_mod { get; set; }

        public Int16 borrado { get; set; }

    }
}
