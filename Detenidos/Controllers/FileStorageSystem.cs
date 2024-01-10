using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Detenidos.Utilidades.Interfaces;
using Detenidos.Models;
using Azure;
using Microsoft.AspNetCore.Hosting;

namespace Detenidos.Utilidades
{
	public class FileStorageSystem : IFileStorage
	{
		private readonly string _connectionString;
		private readonly IWebHostEnvironment _env;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public FileStorageSystem(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
		{
			_connectionString = configuration.GetConnectionString("AzureBlobStorage");
			_env = env;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<StatusFileStorage> SaveFileAzure(string container, IFormFile file, string fileName)
		{
			StatusFileStorage status = new();
			try
			{
				var cliente = new BlobContainerClient(_connectionString, container);
				await cliente.CreateIfNotExistsAsync();
				cliente.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

				var blob = cliente.GetBlobClient(fileName);
				await blob.UploadAsync(file.OpenReadStream());
				
				status.Path = blob.Uri.ToString();
				status.Message = "";
				status.Status = true;
			}
			catch (RequestFailedException rfe)
			{
				status.Path = "";
				status.Message = rfe.Message;
				status.Status = false;
			}
			return status;
		}

		public async Task DeleteFileAzure(string path, string container)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			var cliente = new BlobContainerClient(_connectionString, container);
			await cliente.CreateIfNotExistsAsync();
			var archivo = Path.GetFileName(path);
			var blob = cliente.GetBlobClient(archivo);
			await blob.DeleteIfExistsAsync();
		}

		public async Task<StatusFileStorage> EditFileAzure(string container, IFormFile file, string path, string fileName)
		{
			await DeleteFileAzure(path, container);
			return await SaveFileAzure(container, file, fileName);
		}

		public async Task<StatusFileStorage> SaveFileLocal(string path, IFormFile file)
		{
			StatusFileStorage status = new();
			try
			{
				status.Path = "";
				status.Message = "";
				status.Status = true;
			}
			catch (Exception e)
			{
				status.Path = "";
				status.Message = e.Message;
				status.Status = false;
			}

			return status;
		}

		public async Task<StatusFileStorage> SaveFileWWWRoot(string path, IFormFile file)
		{
			StatusFileStorage status = new();
			try
			{
				string extension = Path.GetExtension(file.FileName);
				string nombreArchivo = $"{Guid.NewGuid()}{extension}";
				string folder = Path.Combine(_env.WebRootPath, path);

				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}

				string ruta = Path.Combine(folder, nombreArchivo);
				using (MemoryStream memoryStream = new())
				{
					await file.CopyToAsync(memoryStream);
					byte[] contenido = memoryStream.ToArray();
					await File.WriteAllBytesAsync(ruta, contenido);
				}

				string urlActual = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
				string urlDB = Path.Combine(urlActual, path, nombreArchivo).Replace("\\", "/");

				status.Path = urlDB;
				status.Message = "";
				status.Status = true;
			}
			catch (Exception e)
			{
				status.Path = "";
				status.Message = e.Message;
				status.Status = false;
			}
			
			return status;
		}
	}
}
