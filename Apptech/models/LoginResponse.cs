namespace Apptech.Models;



    public class LoginResponse
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }