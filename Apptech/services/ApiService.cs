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
    
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://tfgbacken-production.up.railway.app");
        _httpClient.Timeout = TimeSpan.FromSeconds(10); 
    }

    // NUEVO: Método para no escribir el Token 20 veces
    private void AplicarToken()
    {
        var token = Preferences.Get("token", "");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
        }
    }

public async Task<bool> CrearProducto(
    string nombre, 
    string descripcion, 
    decimal precio, 
    Stream archivoStream, 
    string nombreArchivoOriginal, 
    int usuarioId, 
    int categoriaId,
    string estado,          
    string caracteristicas  
)
{
    try
    {
        var token = Preferences.Get("token", "");
        var request = new HttpRequestMessage(HttpMethod.Post, "api/productos");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(nombre ?? ""), "nombre");
        content.Add(new StringContent(descripcion ?? ""), "descripcion");
        
    
        content.Add(new StringContent(caracteristicas ?? "Sin especificar"), "caracteristicas"); 
        content.Add(new StringContent(estado ?? "Nuevo"), "estado_producto");

        content.Add(new StringContent(precio.ToString(System.Globalization.CultureInfo.InvariantCulture)), "precio");
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
            var contenidoError = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("!!!! ERROR SERVIDOR (CREAR): " + contenidoError);
        }
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex) 
    { 
        Debug.WriteLine($"!!!! ERROR EXCEPCIÓN: {ex.Message}");
        return false; 
    }
}
public async Task<List<Producto>> ObtenerProductosVendidos(string token)
{
    try {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
        
       
        var result = await _httpClient.GetFromJsonAsync<List<Producto>>("api/productos/vendidos");
        return result ?? new List<Producto>();
    } 
    catch (Exception ex) { 
        Debug.WriteLine($"Error API: {ex.Message}");
        return new List<Producto>(); 
    }
}
    
  public async Task<bool> ActualizarProducto(int id, string nombre, string descripcion, decimal precio, int categoriaId, Stream archivoStream, string nombreArchivo, string estado, string caracteristicas)
{
    try
    {
        AplicarToken();
        var content = new MultipartFormDataContent();

        
        content.Add(new StringContent(id.ToString()), "Id"); 
        content.Add(new StringContent(nombre), "Nombre");
        content.Add(new StringContent(descripcion ?? ""), "Descripcion");
        content.Add(new StringContent(precio.ToString(CultureInfo.InvariantCulture)), "Precio");
        content.Add(new StringContent(categoriaId.ToString()), "CategoriaId"); // <-- Antes era categoria_id
        content.Add(new StringContent(estado ?? "Nuevo"), "Estado_producto");
        content.Add(new StringContent(caracteristicas ?? ""), "Caracteristicas");

        if (archivoStream != null && !string.IsNullOrEmpty(nombreArchivo))
        {
            var imageContent = new StreamContent(archivoStream);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "ImagenFile", nombreArchivo); // Asegúrate si en el back se llama ImagenFile o ImagenUrl
        }

        
        var response = await _httpClient.PutAsync($"api/productos/{id}", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"❌ Error Servidor ({response.StatusCode}): {error}");
        }

        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error Crítico: {ex.Message}");
        return false;
    }
}
    public async Task<bool> ReservarProductoAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/Productos/reservar/{id}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ItemPop>> ObtenerProductos() 
{
    try 
    {
        
        var response = await _httpClient.GetAsync("api/Productos");
        System.Diagnostics.Debug.WriteLine($"STATUS CODE: {response.StatusCode}");
        if (response.IsSuccessStatusCode)
        {
            
            var productos = await response.Content.ReadFromJsonAsync<List<ItemPop>>();
            return productos ?? new List<ItemPop>();
        }
        
        
        return new List<ItemPop>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en ObtenerProductos: {ex.Message}");
        return new List<ItemPop>();
    }
}

    public async Task<List<Producto>> ObtenerProductosUsuario(string token)
    {
        try {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/productos/mis-productos");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return new List<Producto>();
            return System.Text.Json.JsonSerializer.Deserialize<List<Producto>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        } catch { return new List<Producto>(); }
    }
    
   public async Task<LoginResponse?> Login(string email, string contrasena)
{
    try {
        
        var datos = new { Email = email, Password = contrasena }; 
        
        var response = await _httpClient.PostAsJsonAsync("api/usuarios/login", datos);
        
        if (!response.IsSuccessStatusCode) 
        {
           
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"!!!! LOGIN FALLIDO: {error}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        
return System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);
    } 
    catch (Exception ex) 
    { 
        Debug.WriteLine($"!!!! ERROR DE RED EN LOGIN: {ex.Message}");
        return null; 
    }
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
    try
    {
        
        var response = await _httpClient.GetAsync("api/Categorias");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            return System.Text.Json.JsonSerializer.Deserialize<List<Categoria>>(content, _jsonOptions);
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error obteniendo categorías: {ex.Message}");
    }
    return new List<Categoria>(); 
}
    public async Task<Usuario> ObtenerPerfil(string token)
{
    try 
    {
        AplicarToken(); 
        var response = await _httpClient.GetAsync("api/usuarios/perfil");

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Usuario>(json, _jsonOptions);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"!!!! ERROR PERFIL: {ex.Message}");
        return null; 
    }
}

    public async Task<bool> EliminarProducto(int id)
{
    try
    {
        AplicarToken(); 
        
        var response = await _httpClient.DeleteAsync($"api/productos/{id}");
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"❌ Error al eliminar ({response.StatusCode}): {error}");
        }

        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error Crítico al eliminar: {ex.Message}");
        return false;
    }
}

    

public async Task<string> SubirImagenPerfil(Stream archivoStream, string nombreArchivoOriginal, string token)
{
    try 
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/usuarios/upload-profile-image");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

        var content = new MultipartFormDataContent();
        
        if (archivoStream != null) 
        {
            var fileContent = new StreamContent(archivoStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "imagen", nombreArchivoOriginal);
        } 
        else return null;

        request.Content = content;

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            
            var json = await response.Content.ReadAsStringAsync();
            
            
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("url").GetString();
        }
        
        return null;
    } 
    catch (Exception ex) 
    { 
        Debug.WriteLine($"Error al subir imagen: {ex.Message}");
        return null; 
    }
}
    public async Task<bool> AñadirFavorito(int productoId)
    {
        try
        {
            var token = Preferences.Get("token", "");
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/favoritos/{productoId}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
        public async Task<List<Producto>> ObtenerFavoritos(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/favoritos");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Trim());

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new List<Producto>();

        var json = await response.Content.ReadAsStringAsync();

        return System.Text.Json.JsonSerializer.Deserialize<List<Producto>>(json, 
    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
    ?? new List<Producto>();
    }
    
public async Task<int> ObtenerOCrearChat(int vendedorId, int productoId)
{
    try
    {
        var compradorId = Preferences.Get("userId", 0);
        
        
        Debug.WriteLine($" Mandando Vendedor: {vendedorId}, Comprador: {compradorId}, Producto: {productoId}");

        var datos = new { 
            vendedor_id = vendedorId, 
            comprador_id = compradorId, 
            producto_id = productoId 
        };

        var response = await _httpClient.PostAsJsonAsync("api/mensajes/get-or-create", datos);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("id_chat").GetInt32();
        }
        else 
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($" Error del servidor: {errorBody}");
        }
    }
    catch (Exception ex) { Debug.WriteLine($" Excepción: {ex.Message}"); }
    return 0;
}


public async Task<int> ObtenerChatId(int vendedorId, int productoId)
{
    try
    {
        var compradorId = Preferences.Get("userId", 0);
        var datos = new { vendedor_id = vendedorId, comprador_id = compradorId, producto_id = productoId };
        
        
        var response = await _httpClient.PostAsJsonAsync("api/mensajes/get-or-create", datos);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("id_chat").GetInt32();
        }
    }
    catch (Exception ex) { Debug.WriteLine("!!!! ERROR AL OBTENER CHAT: " + ex.Message); }
    return 0;
}


public async Task<List<Mensaje>> ObtenerMensajes(int chatId)
{
    try
    {
        
        var response = await _httpClient.GetAsync($"api/mensajes/{chatId}");
        if (!response.IsSuccessStatusCode) return new List<Mensaje>();
        
        var json = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<List<Mensaje>>(json, 
    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
    ?? new List<Mensaje>();
    }
    catch { return new List<Mensaje>(); }
}


public async Task<bool> EnviarMensaje(int chatId, string texto)
{
    try
    {
        
        var datos = new 
        { 
            Chat_id = chatId, 
            EmisorId = Preferences.Get("userId", 0), 
            Contenido = texto,
            Fecha = DateTime.Now
            
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/mensajes", datos);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"❌ Error al enviar mensaje: {error}");
        }

        return response.IsSuccessStatusCode;
    }
    catch (Exception ex) 
    { 
        System.Diagnostics.Debug.WriteLine($"❌ Excepción enviando mensaje: {ex.Message}");
        return false; 
    }
}
public async Task<bool> ActualizarEstadoProducto(int productoId, int nuevoEstado)
{
    try 
    {
        
        var response = await _httpClient.PutAsJsonAsync($"api/productos/{productoId}/estado", nuevoEstado);
        return response.IsSuccessStatusCode;
    } 
    catch 
    { 
        return false; 
    }
}
public async Task<List<ChatBandeja>> ObtenerBandejaEntrada(int userId)
{
    try
    {
        var response = await _httpClient.GetAsync($"api/mensajes/mis-chats/{userId}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            
           
            return JsonSerializer.Deserialize<List<ChatBandeja>>(json, _jsonOptions) 
                   ?? new List<ChatBandeja>();
        }
        return new List<ChatBandeja>();
    }
    catch 
    { 
        return new List<ChatBandeja>(); 
    }
}
public async Task<bool> AceptarReservaProducto(int productoId, int compradorId)
{
    try
    {
       
        var response = await _httpClient.PutAsJsonAsync($"api/productos/{productoId}/aceptar-reserva", compradorId);
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error al aceptar reserva: {ex.Message}");
        return false;
    }
}
public async Task<bool> CancelarReservaProducto(int productoId)
{
    try
    {
        AplicarToken(); 

        
        var response = await _httpClient.PutAsync($"api/productos/{productoId}/cancelar-reserva", null);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            
            string errorDetallado = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"❌ Error API al cancelar ({response.StatusCode}): {errorDetallado}");
            return false;
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error de red al cancelar: {ex.Message}");
        return false;
    }
}
public async Task<bool> ConfirmarVentaProducto(int productoId)
{
    try
    {
        AplicarToken();
        
        var response = await _httpClient.PutAsync($"api/productos/{productoId}/confirmar-venta", null);
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error al confirmar venta: {ex.Message}");
        return false;
    }
}
public async Task<ItemPop> GetProductoById(int id)
{
    try
    {
        
        var response = await _httpClient.GetAsync($"api/productos/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            return JsonSerializer.Deserialize<ItemPop>(content, _jsonOptions);
        }
        else 
        {
            var error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"❌ Error API ({response.StatusCode}): {error}");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Excepción: {ex.Message}");
    }
    return null;
}
}