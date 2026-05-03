using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json; 
using System.Globalization;
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
        // Usamos 10.0.2.2 para que el emulador de Android vea el localhost de tu PC
        _httpClient.BaseAddress = new Uri("http://10.0.2.2:5062/");
        _httpClient.Timeout = TimeSpan.FromSeconds(15); 
    }

    // --- CREAR PRODUCTO (SIN CARACTERÍSTICAS NI ESTADO) ---
    public async Task<bool> CrearProducto(
    string nombre, 
    string descripcion, 
    decimal precio, 
    Stream archivoStream, 
    string nombreArchivoOriginal, 
    int usuarioId, 
    int categoriaId)
{
    try
    {
        var token = Preferences.Get("token", "");
        var request = new HttpRequestMessage(HttpMethod.Post, "api/productos");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(nombre ?? ""), "nombre");
        content.Add(new StringContent(descripcion ?? ""), "descripcion");
        
        // Datos fijos para el servidor
        content.Add(new StringContent("Sin especificar"), "caracteristicas"); 
        content.Add(new StringContent("Nuevo"), "estado_producto");

        content.Add(new StringContent(precio.ToString(CultureInfo.InvariantCulture)), "precio");
        content.Add(new StringContent(usuarioId.ToString()), "usuarioId");
        content.Add(new StringContent(categoriaId.ToString()), "categoriaId");

        if (archivoStream != null)
        {
            var fileContent = new StreamContent(archivoStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "imagen", nombreArchivoOriginal ?? "producto.jpg");
        }

        request.Content = content;
       var response = await _httpClient.SendAsync(request);

if (!response.IsSuccessStatusCode)
{
    // LEEMOS EL ERROR REAL
    var contenidoError = await response.Content.ReadAsStringAsync();
    // LO LANZAMOS A LA CONSOLA PARA QUE NO HAYA DUDA
    Debug.WriteLine("!!!! ERROR SERVIDOR (CREAR): " + contenidoError);
    
    // OPCIONAL: Si quieres que salte en el móvil, tendrías que lanzarlo desde la Page, 
    // pero por ahora asegúrate de mirar la pestaña "SALIDA" filtrando por "!!!!"
}
return response.IsSuccessStatusCode;
    }
    catch { return false; }
}
    // --- ACTUALIZAR PRODUCTO (SIN CARACTERÍSTICAS NI ESTADO) ---
  public async Task<bool> ActualizarProducto(
    int id,
    string nombre,
    string descripcion,
    decimal precio,
    int categoriaId,
    Stream imagen,
    string nombreArchivo)
{
    try 
    {
        var token = Preferences.Get("token", "");
        var request = new HttpRequestMessage(HttpMethod.Put, $"api/productos/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

        var content = new MultipartFormDataContent();
        
        // Datos básicos (Siempre se mandan)
        content.Add(new StringContent(id.ToString()), "Id"); 
        content.Add(new StringContent(nombre ?? ""), "Nombre");
        content.Add(new StringContent(descripcion ?? ""), "Descripcion");
        content.Add(new StringContent(precio.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Precio");
        content.Add(new StringContent(categoriaId.ToString()), "CategoriaId");
        
        // Campos de relleno para que el modelo no de error 400
        content.Add(new StringContent("Sin especificar"), "Caracteristicas");
        content.Add(new StringContent("Nuevo"), "Estado_Producto");

        // 🔥 LA CLAVE: Solo añade la imagen al paquete SI el usuario seleccionó una nueva
        if (imagen != null && imagen.Length > 0)
        {
            var fileContent = new StreamContent(imagen);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "Imagen", nombreArchivo ?? "foto.jpg");
        }

        request.Content = content;
        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"!!!! ERROR SERVER: {errorBody}");
        }

        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine("!!!! FALLO CONEXIÓN: " + ex.Message);
        return false;
    }
}
    public async Task<bool> ReservarProductoAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/Productos/reservar/{id}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Producto>> ObtenerProductos()
    {
        var response = await _httpClient.GetAsync("api/productos");
        if (!response.IsSuccessStatusCode) return new List<Producto>();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Producto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Producto>();
    }

    public async Task<List<Producto>> ObtenerProductosUsuario(string token)
    {
        try {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/productos/mis-productos");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return new List<Producto>();
            return JsonSerializer.Deserialize<List<Producto>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Producto>();
        } catch { return new List<Producto>(); }
    }
    
    public async Task<LoginResponse?> Login(string email, string contrasena)
    {
        try {
            var datos = new { Email = email, Contrasena = contrasena };
            var response = await _httpClient.PostAsJsonAsync("api/usuarios/login", datos);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        } catch { return null; }
    } 

    public async Task<bool> Register(Usuario usuario)
    {
        try {
            var response = await _httpClient.PostAsJsonAsync("api/usuarios", usuario);
            return response.IsSuccessStatusCode;
        } catch { return false; }
    }

    public async Task<List<Categoria>> ObtenerCategorias()
    {
        var response = await _httpClient.GetAsync("api/categorias");
        if (!response.IsSuccessStatusCode) return new List<Categoria>();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Categoria>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Categoria>();
    }

    public async Task<Usuario> ObtenerPerfil(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/usuarios/perfil");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Usuario>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> EliminarProducto(int id)
    {
        try {
            var token = Preferences.Get("token", "");
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/productos/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        } catch { return false; }
    }

    public async Task<string> SubirImagenPerfil(Stream archivoStream, string nombreArchivoOriginal, string token)
    {
        try {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/usuarios/upload-profile-image");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            var content = new MultipartFormDataContent();
            if (archivoStream != null) {
                var fileContent = new StreamContent(archivoStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "imagen", nombreArchivoOriginal);
            } else return null;
            request.Content = content;
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? result.Trim('"') : null;
        } catch { return null; }
    }
}