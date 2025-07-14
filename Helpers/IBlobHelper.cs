using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace GestaoEscolarWeb.Helpers
{
    public interface IBlobHelper
    {
        //Faz upload da imagem pelo computador, parametros: ficheiro da web e container onde vai ser guardada a imagem, retorna um guid identificador do blob
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName);


        //Faz upload da imagem, parametros: caminho da imagem e container onde vai ser guardada a imagem, retorna um guid identificador do blob
        Task<Guid> UploadBlobAsync(string image, string containerName);

    }
}
