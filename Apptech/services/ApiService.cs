using System.Net.Http.Headers;
using System.Text.Json;
using System.Globalization  ;
using Microsoft.Maui.Storage;

namespace Apptech.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://192.168.1.137:5062/");
    }

    // Crear producto con imagen
   public async Task CrearProducto(string nombre, string descripcion, decimal precio, Stream archivoStream, string nombreArchivoOriginal)
{
    using var content = new MultipartFormDataContent();

    // Campos de texto
    content.Add(new StringContent(nombre), "nombre");
    content.Add(new StringContent(descripcion), "descripcion");
    content.Add(new StringContent(precio.ToString(CultureInfo.InvariantCulture)), "precio");

    // Archivo (imagen) subido por el usuario
    if (archivoStream != null)
    {
        var fileContent = new StreamContent(archivoStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        // El nombre que pasas aquí solo sirve para que IFormFile tenga extensión, 
        // el backend generará un nombre único
        content.Add(fileContent, "imagen", nombreArchivoOriginal);
    }

    // Llamada a la API
    var response = await _httpClient.PostAsync("api/productos", content);

    if (!response.IsSuccessStatusCode)
    {
        var mensajeError = await response.Content.ReadAsStringAsync();
        throw new Exception($"Error al crear producto: {mensajeError}");
    }
}
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