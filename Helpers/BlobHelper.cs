using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GestaoEscolarWeb.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly CloudBlobClient _blobClient;  //isto que me liga ao contentor

        public BlobHelper(IConfiguration configuration)
        {
            string keys = configuration["Blob:ConnectionString"]; //usado para buscar dados no appsettings

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(keys);  //manda o connection string para se ligar ao container

            _blobClient = storageAccount.CreateCloudBlobClient(); //ligação ao container
        }

        /// <summary>
        /// Asynchronously uploads a file from an <see cref="IFormFile"/> to a specified Azure Blob Storage container.
        /// A new unique GUID will be generated as the blob name.
        /// </summary>
        /// <param name="file">The "IFormFile" representing the file to upload.</param>
        /// <param name="containerName">The name of the blob container where the file will be uploaded.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing a "Guid" representing the unique name assigned to the uploaded blob.
        /// </returns>
        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            Stream stream = file.OpenReadStream();//busca ficheiro
            return await UploadStreamAsync(stream, containerName); //manda ficheiro e nome do container onde será feito upload 
        }


        /// <summary>
        /// Asynchronously uploads a file from a local file path to a specified Azure Blob Storage container.
        /// A new unique GUID will be generated as the blob name.
        /// </summary>
        /// <param name="image">The full path to the local image file to upload.</param>
        /// <param name="containerName">The name of the blob container where the file will be uploaded.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing a "Guid" representing the unique name assigned to the uploaded blob.
        /// </returns>
        public async Task<Guid> UploadBlobAsync(string image, string containerName)
        {
            Stream stream = File.OpenRead(image);
            return await UploadStreamAsync(stream, containerName);
        }


        /// <summary>
        /// Private helper method to perform the actual blob upload from a stream.
        /// It generates a unique GUID for the blob name and uploads the stream content.
        /// </summary>
        /// <param name="stream">The "Stream" containing the content to upload.</param>
        /// <param name="containerName">The name of the blob container where the stream content will be uploaded.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing a "Guid" representing the unique name assigned to the uploaded blob.
        /// </returns>
        private async Task<Guid> UploadStreamAsync(Stream stream, string containerName)
        {
            Guid name = Guid.NewGuid();

            CloudBlobContainer container = _blobClient.GetContainerReference(containerName); //ligação ao blob usando o nome do container

            CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{name}"); //passa o nome gerado pelo guid

            await blockBlob.UploadFromStreamAsync(stream); //faz upload

            return name; //retiorna nome gerado pelo guid
        }
    }
}
