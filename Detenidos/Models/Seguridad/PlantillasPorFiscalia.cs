using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    [Table("SENAP_Carga_PlantillasPorFiscaliaBitacora")]
    public class SENAP_Carga_PlantillasPorFiscaliaBitacora
    {
        [Key]
        public int PlantillasPorFiscaliaBitacoraID { get; set; }
        public int PlantillasPorFiscaliaID { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public bool PlantillaActiva { get; set; }
        public string UserID { get; set; }
        public bool Borrado { get; set; }
    }


    public class PlantillasPorFiscalia
	{
        [Key]
        public int CatFiscaliaID { get; set; }
        public string NombreFiscalia { get; set; }
        public int CatEntidadFederativaID { get; set; }
        public string Mnemonico { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int TotalPlantillas { get; set; }
    }
}
