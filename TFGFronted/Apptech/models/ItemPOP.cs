namespace Apptech.Models
{
    public class ItemPop
    {
        public int Id { get; set; }           // ID del producto
        public int VendedorId { get; set; }   // ID del vendedor
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int precio { get; set; }
        public string ImagenUrl { get; set; }
    }
}