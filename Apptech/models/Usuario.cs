namespace Apptech.Models;
using System.Text.Json.Serialization;
public class Usuario
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Contrasena { get; set; }
    public string? Direccion { get; set; }

   [JsonPropertyName("perfil_url")] 
    public string? PerfilUrl { get; set; }

    [JsonPropertyName("foto_perfil")] 
    public string? FotoPerfil { get; set; }
    public string? Telefono { get; set; }
}