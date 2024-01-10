using System;
using System.Security.Cryptography;
using System.Text;

namespace Detenidos.Utilidades
{
    public class Security
    {
        /// <summary>
        /// Una cadena pseudoaleatoria de donde se generara la encriptacion
        /// </summary>
        /// <remarks>Puede ser de cualquier tamaño</remarks>
        private const string iFrasePasswd = "c1e_ws5ar8dn2hvz6rykmf3i9lt7oqg0u4";

        /// <summary>
        /// Valor para generar la llave de encriptacion.
        /// </summary>
        /// <remarks>Puede ser de cualquier tamaño</remarks>
        private const string iValor = "456^%43:;2323'32-0{][843";

        /// <summary>
        /// Nombre del Algoritmo.
        /// </summary>
        /// <remarks>Puede ser MD5 o SHA1. SHA1 es un poco mas lento pero es mas seguro</remarks>
        private const string iAlgHash = "MD5";

        /// <summary>
        /// Numero de Iteraciones.
        /// </summary>
        /// <remarks>1 o 2 iteraciones son suficientes</remarks>
        private const int iNumIteraciones = 1;

        /// <summary>
        /// Vector Inicial
        /// </summary>
        /// <remarks>Debe ser de 16 caracteres exactamente</remarks>
        private const string iVectorInicial = "4587hst'3smd(@#&amp;";

        /// <summary>
        /// Tamaño de la Llave
        /// </summary>
        /// <remarks>Puede ser de 128, 192 y 256</remarks>
        private const int iTamLlave = 128;

        /// <summary>
        /// Encripta con el algoritmo TripleDES
        /// </summary>
        /// <param name="cadena">Cadena a encriptar</param>
        /// <returns>Cadena encriptada</returns>
        public static string Encrypt(string cadena)
        {
            byte[] resultados;

            UTF8Encoding utf8 = new UTF8Encoding();
            MD5CryptoServiceProvider provHash = new MD5CryptoServiceProvider();
            byte[] llaveTDES = provHash.ComputeHash(utf8.GetBytes(iFrasePasswd));
            TripleDESCryptoServiceProvider algTDES = new TripleDESCryptoServiceProvider()
            {
                Key = llaveTDES,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            // Obtenemos el array de bytes de nuestra cadena a tratar
            byte[] datoEncriptar = utf8.GetBytes(cadena);
            try
            {
                // Generemos en encriptador para nuestro proceso
                ICryptoTransform encriptador = algTDES.CreateEncryptor();
                resultados = encriptador.TransformFinalBlock(datoEncriptar, 0, datoEncriptar.Length);
            }
            finally
            {
                // Liberemos los recursos
                algTDES.Clear();
                provHash.Clear();
            }

            return (Convert.ToBase64String(resultados)).Replace('/', '-').Replace('+', '_');
        }

        /// <summary>
        /// Desencripta con el algoritmo TripleDES
        /// </summary>
        /// <param name="cadena">Cadena a desencriptar</param>
        /// <returns>Cadena desencriptada</returns>
        public static string Decrypt(string cadena)
        {
            cadena = cadena.Replace('-', '/').Replace('_', '+');
            byte[] resultados;
            UTF8Encoding utf8 = new UTF8Encoding();
            MD5CryptoServiceProvider provHash = new MD5CryptoServiceProvider();
            byte[] llaveTDES = provHash.ComputeHash(utf8.GetBytes(iFrasePasswd));
            TripleDESCryptoServiceProvider algTDES = new TripleDESCryptoServiceProvider()
            {
                Key = llaveTDES,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            byte[] datoADesencriptar = Convert.FromBase64String(cadena);
            try
            {
                ICryptoTransform desencr = algTDES.CreateDecryptor();
                resultados = desencr.TransformFinalBlock(datoADesencriptar, 0, datoADesencriptar.Length);
            }
            finally
            {
                algTDES.Clear();
                provHash.Clear();
            }

            return utf8.GetString(resultados);
        }
    }
}
