namespace Proyecto_HIGIA.Models
{
    public class Subcapitulo
    {
        public Int64 id { get; set; }
        public string cod_capitulo { get; set; }
        public string cod_subcapitulo { get; set; }
        public string nom_subcapitulo { get; set; }
        public string nom_capitulo { get; set; }

        public int fk_capitulo { get; set; }
        public double importe { get; set; }
        public DateOnly fh_mod { get; set; }
    }
}
