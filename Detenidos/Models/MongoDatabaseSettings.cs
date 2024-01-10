using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class MongoDatabaseSettings: IMongoDatabaseSettings
    {
        public string MongoConectionString { get; set; }
        public string MongoDatabaseName { get; set; }
    }
    public interface IMongoDatabaseSettings
    {
        string MongoConectionString { get; set; }
        string MongoDatabaseName { get; set; }
    }
}
