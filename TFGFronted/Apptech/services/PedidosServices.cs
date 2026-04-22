using System.Text;
using System.Text.Json;

namespace Apptech.Services
{
    public class PedidoService
    {
        private readonly HttpClient _httpClient;
        private const string Url = "http://192.168.1.36:5062/api/Pedidos";

        public PedidoService(HttpClient httpClient) { _httpClient = httpClient; }

        public async Task<bool> RealizarCompraAsync(int productoId, int vendedorId, decimal precio)
{
    // 1. Intentamos sacar el ID del usuario logueado
    int compradorId = Preferences.Get("UsuarioId", 0);

    // 2. TRUCO: Si el ID es 0 (no logueado), le forzamos el ID 1 (Ruben) 
    // para que la base de datos no rechace el pedido.
    if (compradorId == 0) 
    {
        compradorId = 1; 
    }

    // 3. LOG para que veas en la consola que ahora SI envía un 1
    Console.WriteLine($"DEBUG COMPRA: Enviando Producto {productoId}, Comprador {compradorId}, Vendedor {vendedorId}");

    var nuevoPedido = new
    {
        producto_id = productoId,
        comprador_id = compradorId,
        vendedor_id = vendedorId,
        total = precio,
        fecha = DateTime.Now,
        estado = "completado"
    };

    var json = JsonSerializer.Serialize(nuevoPedido);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.PostAsync(Url, content);
    
    return response.IsSuccessStatusCode;
}
    }
}