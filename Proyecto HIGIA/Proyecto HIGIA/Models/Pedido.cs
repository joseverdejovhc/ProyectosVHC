namespace Proyecto_HIGIA.Models
{
    public class Pedido
    {
        public Int64 id { get; set; }
        public string num_pedido { get; set; }
        public DateOnly fecha_pedido { get; set; }

        public string cod_proveedor { get; set; }
        public string nom_proveedor { get; set; }
        public string cod_acceso_proveedor { get; set; }
        public string usu_mod { get; set; }
        public DateOnly fh_mod { get; set; }


    }
}
