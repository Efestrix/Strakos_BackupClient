using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strakos_BackupClient.Entities
{
    public class BackupJob
    {
        [JsonPropertyName("sources")]
        public List<string> Sources { get; set; } = new List<string>();
        [JsonPropertyName("targets")]
        public List<string> Targets { get; set; } = new List<string>();
        [JsonPropertyName("timing")]
        public string Timing { get; set; }
        public BackupMethod Method { get; set; }
        [JsonPropertyName("retention")]
        public BackupRetention Retention { get; set; }
    }
}
