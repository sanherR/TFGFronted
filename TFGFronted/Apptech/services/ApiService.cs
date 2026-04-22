using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json; 
using System.Globalization  ;
using Microsoft.Maui.Storage;
using Apptech.Models;
using System.Diagnostics;

namespace Apptech.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
public ApiService()
{
    _httpClient = new HttpClient();
    // Cambiamos .137 por .36 que es tu IP real de hoy
    _httpClient.BaseAddress = new Uri("http://192.168.1.36:5062/"); 
}
    // Crear producto con imagen
   public async Task CrearProducto(string nombre, string descripcion, decimal precio, Stream archivoStream, string nombreArchivoOriginal, int usuarioId, int categoriaId)
  {

   try
    {
        var token = Preferences.Get("token", "");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/productos");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Trim());

        var content = new MultipartFormDataContent();

        content.Add(new StringContent(nombre), "nombre");
        content.Add(new StringContent(descripcion), "descripcion");
        content.Add(new StringContent(precio.ToString(CultureInfo.InvariantCulture)), "precio");
        content.Add(new StringContent(usuarioId.ToString()), "usuarioId");
        content.Add(new StringContent(categoriaId.ToString()), "categoriaId");

        // 📸 IMAGEN (OBLIGATORIA según backend)
        if (archivoStream != null)
        {
            var fileContent = new StreamContent(archivoStream);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("image/jpeg");

            content.Add(fileContent, "imagen", nombreArchivoOriginal);
        }
        else
        {
            Console.WriteLine(" ERROR: imagen null");
        }

        request.Content = content;

        var response = await _httpClient.SendAsync(request);

        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine("STATUS: " + response.StatusCode);
        Console.WriteLine("RESPUESTA: " + result);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(result);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR:");
        Console.WriteLine(ex);
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

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<Producto>>(json, options);
    }
  public async Task<LoginResponse?> Login(string email, string contrasena)
    {
        try
        {
            var datos = new
            {
                Email = email,
                Contrasena = contrasena
            };

            Console.WriteLine("ENVÍO: " + JsonSerializer.Serialize(datos));

            var response = await _httpClient.PostAsJsonAsync("api/usuarios/login", datos);

            var result = await response.Content.ReadAsStringAsync();



            Console.WriteLine("STATUS: " + response.StatusCode);
            Console.WriteLine("RESPUESTA: " + result);


            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return JsonSerializer.Deserialize<LoginResponse>(
                result,
                new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true });
            

        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            return null;
        }
    }   
    public async Task<bool> Register(Usuario usuario)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/usuarios", usuario);

            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            return response.IsSuccessStatusCode;

        }
        catch
        {   
           
            return false;
        }
    }
    public async Task<List<Categoria>> ObtenerCategorias()
    {
        var response = await _httpClient.GetAsync("api/categorias");

        if (!response.IsSuccessStatusCode)
        {
            return new List<Categoria>();
        }
        
        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<Categoria>>(json, options);
    }

    private void AddAuthorizationHeader()
    {
        var token = Preferences.Get("token", "");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}