using Microsoft.Data.SqlClient;
using Detenidos.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Detenidos.Services
{
	public class LayoutManager
	{
		private readonly string _connectionString;
        private readonly int _fid;
        private readonly string _ip;
        private readonly string _id;
        private readonly int _numIntento;
        private readonly int _numCarga;

        public const int UPDATE = 0;
        public const int INSERT = 1;

        public LayoutManager(string connectionString, int fid, string ip, string id, int munIntento, int numCarga)
		{
            _connectionString = connectionString;
            _fid = fid;
            _ip = ip;
            _id = id;
            _numIntento = munIntento;
            _numCarga = numCarga;
        }

        public LayoutManager(string connectionString, int fid)
        {
            _connectionString = connectionString;
            _fid = fid;
        }

        public async Task<int> ProcessLayoutGenAsync()
        {
            int result = 0;

            SqlConnection conn = new() { ConnectionString = _connectionString };

            SqlCommand cmd = new()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "[sp_validacionRN_0]" // Definimos el StoredProcedure de las reglas generales
            };

            // Asignamos los parametros a enviar al StoreProcedure
            cmd.Parameters.AddWithValue("@FiscaliaID", _fid);
            cmd.Parameters.AddWithValue("@Usuario", _id);
            cmd.Parameters.AddWithValue("@IP", _ip);
            cmd.Parameters.AddWithValue("@numIntento", _numIntento);
            cmd.Parameters.AddWithValue("@numCarga", _numCarga);
            cmd.Parameters.Add("@ResultadoRN_0", SqlDbType.Int);
            cmd.Parameters["@ResultadoRN_0"].Direction = ParameterDirection.Output;

            try
            {
                conn.Open(); // Abrimos la conexión
                int i = await cmd.ExecuteNonQueryAsync(); // Ejecutamos el StoreProcedure
                int resultSP = Convert.ToInt32(cmd.Parameters["@ResultadoRN_0"].Value); // Obtenemos el resultado de la ejecución del StoreProcedure
                result = resultSP; // 1-Éxito; 2-Error; 3-Advertencia
            }
            catch (Exception e)
            {
                result = 2;
                System.Diagnostics.Debug.WriteLine("Error general en SP: " + e.Message);
                conn.Close();
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

      
        public int CountLayoutRows(int TipoConteo, int CatPlantillasID)
        {
            int count = 0;

            SqlConnection conn = new() { ConnectionString = _connectionString };

            SqlCommand cmd = new()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "[sp_conteosPlantillas]" // Definimos el StoredProcedure del conteo
            };

            // Asignamos los parametros a enviar al StoreProcedure
            cmd.Parameters.AddWithValue("@FiscaliaID", _fid);
            cmd.Parameters.AddWithValue("@numPlantilla", CatPlantillasID);
            cmd.Parameters.AddWithValue("@numTipoConteo", TipoConteo);
            cmd.Parameters.Add("@Conteo", SqlDbType.Int);
            cmd.Parameters["@Conteo"].Direction = ParameterDirection.Output;

            try
            {
                conn.Open(); // Abrimos la conexión
                int i = cmd.ExecuteNonQuery(); // Ejecutamos el StoreProcedure
                int resultSP = Convert.ToInt32(cmd.Parameters["@Conteo"].Value); // Obtenemos el resultado de la ejecución del StoreProcedure
                count = resultSP; // Conteo de registros
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error en sp_conteosPlantillas: " + e.Message);
                conn.Close();
            }
            finally
            {
                conn.Close();
            }
            return count;
        }
    }
}
