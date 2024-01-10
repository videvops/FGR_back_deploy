using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Detenidos.Models;
using Detenidos.Utilidades.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Net;

namespace Detenidos.Utilidades
{
    public class Utilerias
    {        
        public readonly IConfiguration _configuration;       

        public Utilerias() { }

        public Utilerias(IConfiguration config)
        {
            _configuration = config;           

        }

        public string GetValorConstante(string nombreVariable)
        {
            string valor = "";
            try
            {
                valor = _configuration.GetValue<string>(nombreVariable);
            }
            catch
            {

            }
            return valor;
        }

        public  string GetNombreSistema(string nombreSistema)
        {

            string sistema = _configuration.GetValue<string>(nombreSistema);
            return sistema;
        }

        public int GetCarga()
        {
            return 1;
        }

        public int GetNumIntento(int CatPlantillasID)
        {
            string numIntento = GetOneProperty("SELECT MAX(NumIntento) FROM SENAP_Auditoria_ReglasValidacion WHERE CatPlantillasID = " + CatPlantillasID);
            return GetInt(numIntento);
        }

        public string ToCamelCase(string str)
        {
            if (str != null && !str.Equals(""))
            {
                TextInfo txtInfo = new CultureInfo("es-ES", false).TextInfo;
                return txtInfo.ToTitleCase(str.ToLower());
            }
            else return "";
        }

        public int GetSequence(string module)
        {
            string UserID = GetOneProperty("SELECT NEXT VALUE FOR Sequence" + module);
            return int.Parse(UserID);
        }

        public DateTime GetFechaServidor()
        {
            return Convert.ToDateTime(GetOneProperty("select GETDATE() as fecha"));
        }

        public int GetInt(string num = "")
        {
            int id = 0;

            if (!num.Equals(""))
            {
                bool isParsable = int.TryParse(num, out int NumTmp);
                if (isParsable)
                {
                    if (NumTmp > 0)
                    {
                        id = NumTmp;
                    }
                }
            }

            return id;
        }

        public string GetOneProperty(string sql)
        {
            string valor = "";
            using (SqlConnection conn = new(_configuration.GetConnectionString("DataBaseConnection")))
            {
                SqlCommand cmd = new(sql, conn);
                try
                {
                    conn.Open();
                    valor = cmd.ExecuteScalar().ToString();
                }
                catch (Exception e)
                {
                    valor = "";
                }
            }
            return valor;
        }

        public string[] getColumnsSQLTable(string tableName)
        {
            List<string> listColumns = new();

            try
            {
                using (SqlConnection con = new(_configuration.GetConnectionString("DataBaseConnection")))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT c.name FROM sys.columns c INNER JOIN sys.tables t ON t.object_id = c.object_id AND t.name = @tableName AND t.type = 'U'";
                        cmd.Parameters.AddWithValue("@tableName", tableName);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            listColumns.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception) { }

            return listColumns.ToArray();
        }

       
        public static List<string> ConstructSql(string tableName, IEnumerable<dynamic> values)
        {
            List<string> sqls = new();

            foreach (var record in values)
            {
                string sql = "INSERT INTO " + tableName + " (";
                if (record != null)
                {
                    var valorColumnas = "";
                    foreach (var item in record)
                    {
                        valorColumnas = valorColumnas + item.Key.ToString() + ",";
                    }
                    valorColumnas = valorColumnas.Remove(valorColumnas.Length - 1, 1);
                    sql += valorColumnas + ") VALUES (";

                    bool primerComa = false;
                    foreach (var item in record)
                    {
                        if (primerComa)
                        {
                            sql += ",";
                        }
                        primerComa = true;

                        if (int.TryParse(item.Value.ToString(), out int numberInt))
                        {
                            sql += numberInt.ToString();
                        }
                        else if (float.TryParse(item.Value.ToString(), out float numberFloat))
                        {
                            string i = item.Value.ToString();
                            if (i.Length > 7)
                            {
                                sql += "'" + i + "'";
                            }
                            else
                            {
                                sql += numberFloat.ToString();
                            }
                        }
                        else if (DateTime.TryParse(item.Value.ToString(), out DateTime dateTime))
                        {
                            sql += "'" + dateTime.ToString("yyyy-MM-dd") + "'";
                        }
                        else
                        {
                            string tmp = item.Value.ToString();
                            if (tmp.Equals("NULL") || tmp.Equals(""))
                            {
                                sql += "NULL";
                            }
                            else
                            {
                                sql += "'" + item.Value.ToString() + "'";
                            }
                        }
                    }

                    string finalSQL = sql += ")";
                    sqls.Add(finalSQL);
                }
            }

            return sqls;
        }

        private void InsertIntoSqlServer(List<string> sqls)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DataBaseConnection")))
            {
                connection.Open();
                foreach (string sql in sqls)
                {
                    SqlCommand sqlCommand = new SqlCommand(sql, connection);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ReadFile(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    var dt = new DataTable();
                    dt.Load(dr);
                    return dt;
                }
            }
        }

        public static DataTable ConvertToDatatable<T>(List<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    table.Columns.Add(prop.Name, prop.PropertyType.GetGenericArguments()[0]);
                else
                    table.Columns.Add(prop.Name, prop.PropertyType);
            }

            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static byte[] GenerateCSV(DataTable dt)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.CurrentCulture))
                {
                    // Write columns
                    foreach (DataColumn column in dt.Columns)
                    {
                        csvWriter.WriteField(column.ColumnName);
                    }
                    csvWriter.NextRecord();

                    // Write row values
                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csvWriter.WriteField(row[i]);
                        }
                        csvWriter.NextRecord();
                    }
                } // StreamWriter gets flushed here.

                return memoryStream.ToArray();
            }
        }

        public int GetFolioDetenido(int proceso)
        {
            string Folio = GetOneProperty("EXECUTE dbo.sp_SecuenciaFoliosDetenido "+proceso);
            return int.Parse(Folio);
        }

        public int GetFolioEstatalDetenido(int proceso)
        {
            string Folio = GetOneProperty("EXECUTE dbo.sp_SecuenciaFoliosEstatalesDetenido "+proceso);
            return int.Parse(Folio);
        }

        //--------------- Métodos para el WebAPI. -----------------//
        public HttpResponseMessage Call_WebApi(HttpResponseMessage response, string rutaCatalogo,string webapi_url_base)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string urlServer = webapi_url_base;
                    client.BaseAddress = new Uri(urlServer);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    response = client.GetAsync(rutaCatalogo).Result;
                    client.Dispose();
                }

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public static List<AuditoriaDocumento> GetAuditoriaMongoDocumento( AuditoriaDocumento auditoriaIn, List<AuditoriaDocumento> auditoriasIn)
        {
            if (auditoriasIn !=null)
            {
                auditoriasIn.Add(auditoriaIn);
                return auditoriasIn;
            }
            else
            {
                List<AuditoriaDocumento> auditoriaDoc = new();
                auditoriaDoc.Add(auditoriaIn);
                return auditoriaDoc;
            }      
        }

       public string CalcularNombreUnicoArchivo()
        {
            object locker = new object();
            lock (locker)
            {
                //Thread.Sleep(1000);
                return GetFechaServidor().ToString("yyyyMMddHHmmssf");
            }
        }

        public String CalculaHashMD5(Stream stream)
        {
            // Se crea el algoritmo para el hash.
            HashAlgorithm sha1 = HashAlgorithm.Create("SHA1");

            // Se obtiene el arreglo de bytes que conforman al archivo.
            Byte[] sha1Data = sha1.ComputeHash(stream);
            string hashArchi = BitConverter.ToString(sha1Data).Replace("-", "");

            // Se libera el recurso utilizado.
            sha1.Clear();

            return hashArchi;
        }

        public byte[] ConvertirArchivoABytes(IFormFile archivo)
        {
            // Convertimos el archivo recibido en bytes
            BinaryReader lectorBinario = new BinaryReader(archivo.OpenReadStream());
            byte[] datosBinarios = lectorBinario.ReadBytes((int)archivo.Length);

            MemoryStream flujo = new MemoryStream(datosBinarios);
            byte[] bytesArchivo = new byte[flujo.Length];
            flujo.Read(bytesArchivo, 0, bytesArchivo.Length);
            flujo.Close();

            return bytesArchivo;
        }

        public bool UploadFTP(Byte[] arregloBytes, ConfigFtp datosFtp, string rutaCompletaFtp)
        {           
            using (WebClient request = new WebClient())
            {                            
                request.Credentials = new NetworkCredential(datosFtp.Usuario, Security.Decrypt(datosFtp.Password));
                try
                {                    
                    request.UploadData(rutaCompletaFtp, arregloBytes);
                }
                catch (WebException e)
                {                                     
                    request.Proxy = new WebProxy();
                    request.UploadData(rutaCompletaFtp, arregloBytes);
                }
            }
            return true;
        }

        public Fichas SubirArchivoFTP(IFormFile archivo, ApplicationUser usuario,ConfigFtp datosFtp,string idDetenido)
        {            
            Fichas ficha = new();

            try
            {
                ficha.UsuarioAlta = usuario.Id;
                ficha.NombreUsuarioAlta =usuario.FriendlyName;                
                ficha.NombreTrabajoFicha = "";
                ficha.PersonalID = 0;
                ficha.NombreArchivo = archivo.FileName;
                ficha.TipoArchivo = archivo.ContentType;
                ficha.NombreUnico =idDetenido+"-"+CalcularNombreUnicoArchivo();
                Stream streamArchivo = archivo.OpenReadStream();
                ficha.Hash = CalculaHashMD5(streamArchivo);
                ficha.Observaciones = "";
                ficha.Ip = "";
                ficha.FechaAltaDelta = new Utilerias(_configuration).GetFechaServidor();
                ficha.FechaActualizacionDelta = null;
                ficha.Borrado = 0;
                ficha.RutaFicha = datosFtp.Ruta+datosFtp.Carpeta+ficha.NombreUnico+ficha.NombreArchivo;

                byte[] arregloBytes = ConvertirArchivoABytes(archivo);              
                UploadFTP(arregloBytes,datosFtp,ficha.RutaFicha);                   
            }         
            catch (Exception e)
            {
                ficha = null;
            }            
            return ficha;
        }

        public byte[] DownloadFTP(FichaDTO archivoFicha, ConfigFtp datosFtp)
        {  
            WebClient request = new WebClient();
            byte[] bytesArchivo;        

            request.Credentials = new NetworkCredential(datosFtp.Usuario,Security.Decrypt(datosFtp.Password));
            try
            {
                // Primero se obtiene el arreglo de bytes,
                bytesArchivo = request.DownloadData(archivoFicha.RutaFicha);              
                archivoFicha.Cadena64 = Convert.ToBase64String(bytesArchivo);
               
            }
            catch (WebException)
            {
                return null;
            }            
            return bytesArchivo;
        }

        public List<PFM_CatDelito> GetPFMCatDelito() {
            string valorUrlApi = GetValorConstante("webapi_url_base");
            HttpResponseMessage responsePfmCatDelito = Call_WebApi(new HttpResponseMessage(),
                      "api/SL_Catalogos/GetDelitos", valorUrlApi);

            List<PFM_CatDelito> pfmCatDelito = GetPfmCatDelito(responsePfmCatDelito);
            return pfmCatDelito;
        }

        public PFM_CatDelito GetPFMCatDelitoById(int Id)
        {
            string valorUrlApi = GetValorConstante("webapi_url_base");
            HttpResponseMessage responsePfmCatDelito = Call_WebApi(new HttpResponseMessage(),
                      "api/SL_Catalogos/GetDelitoByID"+"?DelitoID="+Id, valorUrlApi);
            PFM_CatDelito pfmCatDelito = GetPfmCatDelitoById(responsePfmCatDelito);
            return pfmCatDelito;
        }



        public List<PFM_CatDelito> GetPfmCatDelito(HttpResponseMessage response)
        {
            List<PFM_CatDelito> catPfmDelito;
            if (response.IsSuccessStatusCode)
            {
                string cadenaRespuesta = response.Content.ReadAsStringAsync().Result;
                catPfmDelito = JsonConvert.DeserializeObject<List<PFM_CatDelito>>(cadenaRespuesta);
            }
            else { throw new Exception("Error al obtener datos"); }

            return catPfmDelito;
        }

        public PFM_CatDelito GetPfmCatDelitoById(HttpResponseMessage response)
        {
            PFM_CatDelito catPfmDelito;
            if (response.IsSuccessStatusCode)
            {
                string cadenaRespuesta = response.Content.ReadAsStringAsync().Result;
                catPfmDelito = JsonConvert.DeserializeObject<PFM_CatDelito>(cadenaRespuesta);
            }
            else { throw new Exception("Error al obtener datos"); }

            return catPfmDelito;
        }



        public List<PFM_CatDelitoModalidadPrometheus> GetCatDelitoModalidad(int id)
            {
            string valorUrlApi = GetValorConstante("webapi_url_base");
            HttpResponseMessage responseCatDelitoModalidad = Call_WebApi(new HttpResponseMessage(),
                        "api/SL_Catalogos/GetModalidades" + "?DelitoID=" + id, valorUrlApi);

            List<PFM_CatDelitoModalidadPrometheus> pfmCatDelitoModalidad = GetCatDelitoModalidad(responseCatDelitoModalidad);
            return pfmCatDelitoModalidad;
            }


        private static List<PFM_CatDelitoModalidadPrometheus> GetCatDelitoModalidad(HttpResponseMessage response)
        {
            List<PFM_CatDelitoModalidadPrometheus> catDelitoModalidad;
            if (response.IsSuccessStatusCode)
            {
                string cadenaRespuesta = response.Content.ReadAsStringAsync().Result;
                catDelitoModalidad = JsonConvert.DeserializeObject<List<PFM_CatDelitoModalidadPrometheus>>(cadenaRespuesta);
            }
            else { throw new Exception("Error al obtener datos"); }

            return catDelitoModalidad;
        }


        public PFM_CatDelitoModalidadPrometheus GetCatDelitoModalidadClasificacion(int id)
        {
            string valorUrlApi = GetValorConstante("webapi_url_base");
            HttpResponseMessage responseCatDelitoModalidadClasificacion = Call_WebApi(new HttpResponseMessage(),
                        "api/SL_Catalogos/GetModalidadClasificacionByID" + "?catClasificaDelitoID=" + id, valorUrlApi);

            PFM_CatDelitoModalidadPrometheus pfmCatDelitoModalidadClasificacion = GetCatDelitoModalidadClasifica(responseCatDelitoModalidadClasificacion);
            return pfmCatDelitoModalidadClasificacion;
        }


        private static PFM_CatDelitoModalidadPrometheus GetCatDelitoModalidadClasifica(HttpResponseMessage response)
        {
            PFM_CatDelitoModalidadPrometheus catDelitoModalidadClasifica;
            if (response.IsSuccessStatusCode)
            {
                string cadenaRespuesta = response.Content.ReadAsStringAsync().Result;
                catDelitoModalidadClasifica = JsonConvert.DeserializeObject<PFM_CatDelitoModalidadPrometheus>(cadenaRespuesta);
            }
            else { throw new Exception("Error al obtener datos"); }

            return catDelitoModalidadClasifica;
        }



    }
}
