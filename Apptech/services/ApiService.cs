using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Apptech.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://192.168.1.10:5000/");
    }

    // Crear producto con imagen
    public async Task CrearProducto(string nombre, string descripcion, string precio, FileResult archivo)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(nombre), "nombre");
        content.Add(new StringContent(descripcion), "descripcion");
        content.Add(new StringContent(precio), "precio");

        var stream = await archivo.OpenReadAsync();
        content.Add(new StreamContent(stream), "imagen", archivo.FileName);

        var response = await _httpClient.PostAsync("api/productos", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error al crear producto");
        }
    }

    // Obtener productos
    public async Task<List<Producto>> ObtenerProductos()
    {
        var response = await _httpClient.GetAsync("api/productos");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error al obtener productos");
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Producto>>(json);
    }
}