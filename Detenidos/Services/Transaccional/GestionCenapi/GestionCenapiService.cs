using Detenidos.Models;
using LinqKit;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Services
{
    public class GestionCenapiService
    {
        private readonly IMongoCollection<Detenido> _detenidos;
        private readonly IMongoDatabaseSettings _settings;
        private readonly AuditoriaMongoService _auditoriaMongoService;       
        public GestionCenapiService(IMongoDatabaseSettings settings, AuditoriaMongoService auditoriaMongoService)
        {
            var client = new MongoClient(settings.MongoConectionString);
            var database = client.GetDatabase(settings.MongoDatabaseName);

            _detenidos = database.GetCollection<Detenido>("detenidos");
            _auditoriaMongoService = auditoriaMongoService;
            _settings = settings;           
        }
        public async Task<List<Detenido>> Get()
        {           
            List<Detenido> solicitudes = await _detenidos.Find(x=>x.Borrado==0).SortByDescending(x=>x.Id).ToListAsync();           
            return solicitudes;
        }        

    }
}
