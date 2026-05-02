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
        _httpClient.BaseAddress = new Uri("http://192.168.1.137:5062/");
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

        //  IMAGEN (OBLIGATORIA según backend)
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
  public async Task<bool> ActualizarProducto(
    int id,
    string nombre,
    string descripcion,
    decimal precio,
    int categoriaId,
    Stream imagen,
    string nombreArchivo)
{
    var token = Preferences.Get("token", "");

    var request = new HttpRequestMessage(HttpMethod.Put, $"api/productos/{id}");

    request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    var content = new MultipartFormDataContent();

    content.Add(new StringContent(nombre), "nombre");
    content.Add(new StringContent(descripcion), "descripcion");
    content.Add(new StringContent(precio.ToString()), "precio");
    content.Add(new StringContent(categoriaId.ToString()), "categoriaId");

    if (imagen != null)
    {
        var fileContent = new StreamContent(imagen);
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/jpeg");

        content.Add(fileContent, "imagen", nombreArchivo);
    }

    request.Content = content;

    var response = await _httpClient.SendAsync(request);

    return response.IsSuccessStatusCode;
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
    public async Task<List<Producto>> ObtenerProductosUsuario(string token)
{
    try
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/productos/mis-productos");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Trim());

        var response = await _httpClient.SendAsync(request);

        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine("STATUS: " + response.StatusCode);
        Console.WriteLine("RESPUESTA: " + result);

        if (!response.IsSuccessStatusCode)
            return new List<Producto>();

        return JsonSerializer.Deserialize<List<Producto>>(
            result,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Producto>();
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR ObtenerProductosUsuario:");
        Console.WriteLine(ex);
        return new List<Producto>();
    }
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

    public async Task<string> SubirImagenPerfil(Stream archivoStream, string nombreArchivoOriginal, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/usuarios/upload-profile-image");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Trim());

            var content = new MultipartFormDataContent();

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
                return null;
            }

            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine("STATUS: " + response.StatusCode);
            Console.WriteLine("RESPUESTA: " + result);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Se asume que el backend devuelve la URL de la imagen como texto plano
            return result.Trim('"'); // Eliminar comillas si las hay
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR:");
            Console.WriteLine(ex);
            return null;
        }
    }
    public async Task<Usuario> ObtenerPerfil(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/usuarios/perfil");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Trim());

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<Usuario>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    public async Task<bool> EliminarProducto(int id)
{
    try
    {
        var token = Preferences.Get("token", "");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/productos/{id}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Trim());

        var response = await _httpClient.SendAsync(request);

        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine("STATUS: " + response.StatusCode);
        Console.WriteLine("RESPUESTA: " + result);

        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR EliminarProducto:");
        Console.WriteLine(ex);
        return false;
    }
}
    
}