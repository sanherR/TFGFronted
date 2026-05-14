namespace Apptech.Models;

public class Usuario
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Contrasena { get; set; }
    public string? Direccion { get; set; }

    public string? PerfilUrl { get; set; }

    public string? Telefono { get; set; }
}