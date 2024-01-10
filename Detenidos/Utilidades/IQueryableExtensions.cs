using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Detenidos.Models;

namespace Detenidos.Utilidades
{
	public static class IQueryableExtensions
	{
		public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDTO paginacionDTO)
		{
			queryable = queryable.Skip((paginacionDTO.Pagina - 1) * paginacionDTO.RecordsPorPagina);
			if (paginacionDTO.RecordsPorPagina >= 0)
			{
				queryable = queryable.Take(paginacionDTO.RecordsPorPagina);
			}
			return queryable;
		}
	}
}
