namespace Proyecto_HIGIA.Models
{
    public class Factura
    {
        public Int64 id { get; set; }
        public string num_factura { get; set; }
        public string num_expedicion { get; set; }
        public string fh_factura { get; set; }
        public int fk_pedido { get; set; }

        public DateOnly fh_mod { get; set; }
        public string usu_mod { get; set; }

    }
}
