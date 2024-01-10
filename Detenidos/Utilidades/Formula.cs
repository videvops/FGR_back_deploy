using Microsoft.EntityFrameworkCore;
using Detenidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades
{
    public class Formula
    {	
        private static int CalcularConsecutivo(int ultimoFolio, int total)
		{
			int folio = 0;

			if (ultimoFolio == total)
			{
				folio = total + 1;
			}
			else
			{
				ultimoFolio += 1; 
				int ajustador = total - ultimoFolio; 
				folio = total - ajustador; 
			}
			return folio;
		}

	}
}
