using Microsoft.AspNetCore.Http;
using Detenidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades.Interfaces
{
	public interface IFileStorage
	{
		Task<StatusFileStorage> SaveFileAzure(string container, IFormFile file, string fileName);
		Task DeleteFileAzure(string path, string container);
		Task<StatusFileStorage> EditFileAzure(string container, IFormFile file, string path, string fileName);
		Task<StatusFileStorage> SaveFileLocal(string path, IFormFile file);
		Task<StatusFileStorage> SaveFileWWWRoot(string path, IFormFile file);
	}
}
