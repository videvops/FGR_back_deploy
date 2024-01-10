using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class AuditoriaMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string IdUsuario { get; set; }
        public string Coleccion { get; set; }
        public DateTime FechaEvento { get; set; }
        public string IdColeccion { get; set; }
        public string Valores { get; set; }//representa los valores de terminos de busqueda
        public string ValoresResultado { get; set; }//representa los valores que arroja la consulta(total)
        public int OperacionID { get; set; }
        public string Operacion { get; set; }
        public string Ip { get; set; }        
    }

    public class DatosAuditoria
    {
        public string Coleccion { get; set; }
        public string IdColeccion { get; set; }
        public int OperacionID { get; set; }
        public string Operacion { get; set; }
        public string Valores { get; set; }
        public string ValoresResultado { get; set; }
    }

    public class AuditoriaDocumento
    {
        public string IdUsuario { get; set; }
        public string Coleccion { get; set; }
        public DateTime FechaEvento { get; set; }
        public string IdColeccion { get; set; }
        public string Valores { get; set; }
        //public string ValoresViejos { get; set; }
        public int OperacionID { get; set; }
        public string Operacion { get; set; }
        public string Ip { get; set; }

    }

    public class DatosValores
    {
        public string NombreValor { get; set; }
        public string Valor { get; set; }
    }
}
