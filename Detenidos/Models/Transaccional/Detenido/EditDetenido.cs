using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class EditDetenido
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int FolioIngreso { get; set; }
        public int AnioFolioIngreso { get; set; }
        public int CatSeparoID { get; set; }
        public int SedeSubsedeID { get; set; }
        public string NombreSedeSubsede { get; set; }
        public string NombreSedeSubsedeLargo { get; set; }
        public int CatEntidadFederativaID { get; set; }
        public string EntidadFederativa { get; set; }
        public DateTime? FehaHoraIngreso { get; set; }
        public string NombreDetenido { get; set; }
        public string ApellidoPaternoDetenido { get; set; }
        public string ApellidoMaternoDetenido { get; set; }
        public string OtrosNombres { get; set; }
        public string Observaciones { get; set; }
        public int NacionalidadID { get; set; }
        public string Nacionalidad { get; set; }
        public string OficioRetencion { get; set; }
        public int DependenciaDetencionID { get; set; }
        public string DependenciaDetencion { get; set; }
        public string RutaFirmaHuella { get; set; }
        public string UsuarioID { get; set; }
        public DateTime? FechaAltaDelta { get; set; }
        public DateTime? FechaActualizacionDelta { get; set; }
        public List<Aliases> Aliases { get; set; }
        public List<Delitos> Delitos { get; set; }
        public List<Asignacion> Asignacion { get; set; }
        public List<Egreso> Egreso { get; set; }
        public List<Fichas> Fichas { get; set; }
        public List<EstatusRegistro> EstatusRegistro { get; set; }
        public int Borrado { get; set; }

        public List<string> AliasesEdit { get; set; }
        public List<int> DelitosEdit { get; set; }
    }
}
