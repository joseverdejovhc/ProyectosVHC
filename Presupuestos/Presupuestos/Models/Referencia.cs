namespace Presupuestos.Models
{
    public class Referencia
    {
        public Referencia(string cODIRE, string cODALT, string nOMREF, string agrupacion, string tIPREF,string UNISTK, string tIPPRD, string linea, string planta, string euros_ventas, string kilos_ventas, string stock_previsto, string kilos_produccion, string cdv_almacen, string cdf_almacen, string cdf, string cdv)
        {
            this.CODIRE = cODIRE;
            this.CODALT = cODALT;
            this.NOMREF = nOMREF;
            this.agrupacion = agrupacion;
            this.TIPREF = tIPREF;
            this.UNISTK = UNISTK;
            this.TIPPRD = tIPPRD;
            this.linea = linea;
            this.planta = planta;
            this.euros_venta = Convert.ToDouble(checknull(euros_ventas));
            this.kilos_venta = Convert.ToDouble(checknull(kilos_ventas));
            this.stock_previsto = Convert.ToDouble(checknull(stock_previsto));
            this.kilos_produccion = Convert.ToDouble(checknull(kilos_produccion));
            this.cdv_almacen = Convert.ToDouble(checknull(cdv_almacen));
            this.cdf_almacen = Convert.ToDouble(checknull(cdf_almacen));
            this.cdv = Convert.ToDouble(checknull(cdv));
            this.cdf = Convert.ToDouble(checknull(cdf));
        }

        public Referencia()
        {
        }

        public Int64 id { get; set; }
        public int FK_AGR { get; set; }
        public string CODIRE { get; set; }
        public string CODALT { get; set; }
        public string NOMREF { get; set; }
        public string agrupacion { get; set; }
        public string UNISTK { get; set; }

        public string TIPREF { get; set; }
        public string TIPPRD { get; set; }

        public string linea { get; set; }
        public string planta { get; set; }

        public double euros_venta { get; set; }
        public double kilos_venta { get; set; }
        public double stock_previsto { get; set; }
        public double cdv_almacen { get; set; }
        public double cdf_almacen { get; set; }
        public double cdv_cargo_almacen { get; set; }

        public double cdf_cargo_almacen { get; set; }

        public double pmv_kilos { get; set; }
        public double kilos_cargo_almacen { get; set; }

        public double kilos_produccion { get; set; }

        public double kilos_cargo_produccion { get; set; }
        public double cdv { get; set; }
        public double cdf { get; set; } //Debe calcularse con el cdf unitario

        public double cdv_cargo_produccion { get; set; }
        public double cdf_cargo_produccion { get; set; }

        public double cdv_total { get; set; }
        public double cdv_unitario { get; set; }

        public double cdf_total { get; set; }
        public double cdf_unitario { get; set; }
        public double margen_bruto { get; set; }
        public double porcen_margen { get; set; }

        public DateTime FH_ALTA { get; set; }
        public Int16 borrado { get; set; }

        public string checknull(string valor)
        {
            string cadena="0.0";

            if(valor != null && valor != "")
            {
                cadena = valor;
            }
           

            return cadena;
        }
    }
}
