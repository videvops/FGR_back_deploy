using Detenidos.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Services
{
    public class AuditoriaMongoService
    {
        private readonly IMongoCollection<AuditoriaMongo> _auditoriaMongo;

        public AuditoriaMongoService(IMongoDatabaseSettings settings)
        {
            var client = new MongoClient(settings.MongoConectionString);
            var database = client.GetDatabase(settings.MongoDatabaseName);

            _auditoriaMongo = database.GetCollection<AuditoriaMongo>("auditoria");
        }
        public void GuardarAuditoria(DatosUsuarioActualDTO datosUsuarioIn, DatosAuditoria datosIn)
        {
            /*Operaciones para la auditoria en mongo
            * 1-Create
            * 2-Update
            * 3-Delete
            * 4-Search  
            * 5-Action
            */
            AuditoriaMongo auditoria = new();
            auditoria.IdUsuario = datosUsuarioIn.Id;
            auditoria.Coleccion = datosIn.Coleccion;
            auditoria.FechaEvento = datosUsuarioIn.FechaEvento;
            auditoria.IdColeccion = datosIn.IdColeccion;
            auditoria.Valores = datosIn.Valores;
            auditoria.ValoresResultado = datosIn.ValoresResultado;
            auditoria.OperacionID = datosIn.OperacionID;
            auditoria.Operacion = datosIn.Operacion;
            auditoria.Ip = datosUsuarioIn.Ip;
            _auditoriaMongo.InsertOne(auditoria);

        }

    }
}
