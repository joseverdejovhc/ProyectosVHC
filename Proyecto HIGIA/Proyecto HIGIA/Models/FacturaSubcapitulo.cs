namespace Proyecto_HIGIA.Models
{
    public class FacturaSubcapitulo
    {
        public Int64 id { get; set; }
        public int fk_factura { get; set; }
        public int fk_subcapitulo { get; set; }

        public double importe_facturacion { get; set; }

        public DateOnly fh_mod { get; set; }
    }
}
