namespace Presupuestos.Models
{
    public class Seccion
    {
        public Int64 id { get; set; }
        public string texto_modulo { get; set; }
        public string alias { get; set; }

        public DateTime fh_alta { get; set; }

        public DateTime fh_baja { get; set; }

        public Int16 borrado { get; set; }
        public int escritura { get; set; }
    }
}
