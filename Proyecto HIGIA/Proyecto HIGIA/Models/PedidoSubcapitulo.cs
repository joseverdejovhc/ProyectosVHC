namespace Proyecto_HIGIA.Models
{
    public class PedidoSubcapitulo
    {
        public Int64 id { get; set; }
        public int fk_pedido { get; set; }
        public int fk_subcapitulo { get; set; }

        public double importe { get; set; }
        public string usu_mod { get; set; }
        public DateOnly fecha_mod { get; set; }

    }
}
