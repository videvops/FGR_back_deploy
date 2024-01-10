using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Detenidos.Models
{
    [Table("Auditoria")]
    public class Audit
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public DateTime DateTimeEvent { get; set; }
        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public int OperationId { get; set; }
        public string Operation { get; set; }
        public string IP { get; set; }
        public string UserId { get; set; }
    }

    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public DateTime DateTimeEvent { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
        public int OperationId { get; set; }
        public string Operation { get; set; }
        public string IP { get; set; }
        public string UserId { get; set; }

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public Audit ToAudit()
        {
            var audit = new Audit
            {
                TableName = TableName,
                DateTimeEvent = DateTimeEvent, //DateTime.UtcNow,
                KeyValues = JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues), // In .NET Core 3.1+, you can use System.Text.Json instead of Json.NET
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                OperationId = OperationId,
                Operation = Operation,
                IP = IP,
                UserId = UserId

            };
            return audit;
        }
    }
}
