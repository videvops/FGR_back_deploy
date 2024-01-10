using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades.Helpers
{
	public enum EnumCatBitacoraCarga
	{
		CSV_In_SQL = 1,
		Error_BlobStorage = 2,
		Wrong_Structure = 3,
		Empty_Template = 4,
		IdentificadorFiscalia_Incorrect = 5,
		Error_Restore_Layout_SQL = 6,
		Pipeline_Execution_Error = 7,
		Error_Getting_The_Execution_Message = 8,
		Error_Updating_CatFiscaliaID_By_IdentificadorFiscalia = 9,
		Local_Success_CSV_To_SQL = 10,
		Local_Error_CSV_To_SQL = 11
	}
}
