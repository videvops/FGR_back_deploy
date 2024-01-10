using Detenidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades
{
	// Utilidad donde se esncuentran los métodos para generar los nombres de archivos para su inserción en BD y AzureStorage
	// Zurisadai Castro Nava (16/02/2021)
	public class DataFactorySettings
	{
		private readonly ApplicationDbContext context;

		public DataFactorySettings(ApplicationDbContext context)
		{
			this.context = context;
		}

		

		//public string GetBlobStorage(int CatFiscaliaID)
		//{
		//	// blob-ags
		//	return "blob-" + GetMnemonicoFiscalia(CatFiscaliaID).ToLower();
		//}

		

		//public string GetPathCSVAzure(string storageAccount, string blobStorage, string filenameCSV)
		//{
		//	// https:// senapstorage.blob.core.windows.net/blob-ags/File_AGS_NoticiaCriminal.csv
		//	return "https://" + storageAccount + ".blob.core.windows.net/" + blobStorage + "/" + filenameCSV;
		//}

		////public string GetMnemonicoFiscalia(int CatFiscaliaID)
		////{
		////	return context.CatFiscalias.Where(x => x.CatFiscaliaID == CatFiscaliaID).Select(x => x.Mnemonico).FirstOrDefault() ?? "";
		////}
		

		

		//public string GetMessageTranslate(string Mensaje)
		//{
		//	if (Mensaje == "String was not recognized as a valid DateTime.")
		//	{
		//		Mensaje = "La cadena no se reconoció como una fecha y hora válida.";
		//	}
		//	if (Mensaje == "String was not recognized as a valid TimeSpan.")
		//	{
		//		Mensaje = "La cadena no se reconoció como un intervalo de tiempo válido.";
		//	}
		//	if (Mensaje == "A database operation failed. Please search error to get more details.")
		//	{
		//		Mensaje = "Falló una operación de base de datos. Busque el error para obtener más detalles.";
		//	}


		//	if (Mensaje == "Column 'CatTipoAgenciaID' does not allow DBNull.Value.")
		//	{
		//		Mensaje = "Archivo .csv incorrecto";
		//	}
		//	if (Mensaje == "Column 'DelitoID_Fiscalia' does not allow DBNull.Value.")
		//	{
		//		Mensaje = "Archivo .csv incorrecto";
		//	}
		//	if (Mensaje == "Column 'DeterminacionID_Fiscalia' does not allow DBNull.Value.")
		//	{
		//		Mensaje = "Archivo .csv incorrecto";
		//	}
		//	if (Mensaje == "Column 'MandamientoJudicialID' does not allow DBNull.Value.")
		//	{
		//		Mensaje = "Archivo .csv incorrecto";
		//	}
		//	if (Mensaje == "Column 'ProcesoID_Fiscalia' does not allow DBNull.Value.")
		//	{
		//		Mensaje = "Archivo .csv incorrecto";
		//	}
		//	if (Mensaje == "Column 'NoticiaCriminalID_Fiscalia' does not allow DBNull.Value.")
		//	{
		//		Mensaje = "Archivo .csv incorrecto";
		//	}


		//	if (Mensaje == "Input string was not in a correct format.")
		//	{
		//		Mensaje = "Cadena de entrada no tiene el formato correcto.";
		//	}
		//	return Mensaje;
		//}

		//public string GetDataFactoryResponse(string msg, string plantilla)
		//{
		//	string result = "Ocurrió un error al cargar la plantilla '" + plantilla + "'!,";

		//	try
		//	{
		//		if (msg.Contains("DelimitedTextMoreColumnsThanDefined")) // Este caso ya se controla antes de subir el archivo al BlobStorage
		//		{
		//			result = "La estructura de la plantilla '" + plantilla + "' no es la correcta!";
		//		}
		//		if (msg.Contains("SqlOperationFailed"))
		//		{
		//			string[] gen = msg.Split(",Message=Column '");
		//			result = "La columna '" + gen[1].Substring(0, gen[1].IndexOf("'")) + "' de la plantilla '" + plantilla + "', no puede estar vacía!";
		//		}
		//		if (msg.Contains("UserErrorInvalidDataValue"))
		//		{
		//			string[] gen = msg.Split(",Message=Column '");
		//			string[] fin = gen[1].Substring(0, gen[1].IndexOf(".")).Split("'");
		//			result = "El valor '" + fin[2] + "' no es válido para la columna '" + fin[0] + "', de la plantilla '" + plantilla + "'!";
		//		}
		//		if (msg.Contains("Message=Parameter value"))
		//		{
		//			string[] gen = msg.Split("Message=Parameter");
		//			string[] fin = gen[1].Split("'");
		//			result = "El valor '" + fin[1] + "' está fuera del rango permitido, de la plantilla '" + plantilla + "'!";
		//		}
		//	}
		//	catch (Exception)
		//	{
		//		result = "Ocurrió un error al cargar la plantilla '" + plantilla + "'!,";
		//	}

		//	return result;
		//}
	}
}
