using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strakos_BackupClient.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BackupMethod
    {
        Full,
        Differential,
        Incremental
    }
}
