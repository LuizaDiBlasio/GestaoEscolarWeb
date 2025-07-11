using System;

namespace GestaoEscolarWeb.Models
{
    public class TokenResponseModel //modelo para receber o json durante a desserialização 
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
