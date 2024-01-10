using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Detenidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Detenidos.Utilidades;
using Detenidos.Services;

namespace Detenidos.Utilidades
{
    public class Validador
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration _configuration;
        private readonly DetenidoService _detenidoService;

        public Validador(ApplicationDbContext context, DetenidoService detenidoService,IConfiguration configuration = null)
        {
            this.context = context;
            _configuration = configuration;
            _detenidoService = detenidoService;
        }
        public void IntentarAgregarError(Dictionary<string, string[]> dicc, string llave, List<string> valor)
        {
            // Si hay errores agregados a la lista, se agregan al diccionario.
            if (valor.Count > 0)
            {
                dicc.Add(llave, valor.ToArray());
                // Y se deja limpia la lista de errores.
                valor.Clear();
            }
        }

        public async Task<Dictionary<string, string[]>> ValidaEditDetenido(string id)
        {
            Dictionary<string, string[]> diccionario = new();
            string llave = "";
            List<string> listaErrores = new();
            llave = "1";
            var detenido = await _detenidoService.Get(id);
            var estatus = detenido.EstatusRegistro.Where(x=>x.EstatusID==2);

            if (estatus.Count()>0)
            {
                listaErrores.Add("El registro no se puede editar, ya ha sido visualizado por CENAPI");
            }
            IntentarAgregarError(diccionario, llave, listaErrores);
            return diccionario;
        }

    }
}
