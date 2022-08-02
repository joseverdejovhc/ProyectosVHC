namespace Presupuestos.Models
{
    public class Sie
    {

        public Int64 id { get; set; }

        public Int16 fk_centro_coste { get; set; }

        public string codigo { get; set; }
        public string nom_centro_coste { get; set; }
        public string clasificacion { get; set; }
        public string coste_inversion { get; set; }
        public string cuenta { get; set; }
        public string nombre_cuenta { get; set; }
        public double importe { get; set; }
  
        public Int16 borrado { get; set; }
        public int escritura { get; set; }
    }
}
