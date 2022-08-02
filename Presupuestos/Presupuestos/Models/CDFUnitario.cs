namespace Presupuestos.Models
{
    public class CDFUnitario
    {

        public int id { get; set; }
        public string planta { get; set; }
        public double porcentaje_cdf { get; set; }
        public double gasto_repartir { get; set; }
        public double prevision_kilos { get; set; }
        public double cdf_kilos { get; set; }

    }
}
