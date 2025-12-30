using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Strakos_BackupClient
{
    public class QuartzJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            if (!File.Exists("Config.json"))
            {
                Console.WriteLine("Config.json nebyl nalezen");
            }

            string json = File.ReadAllText("Config.json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var jobs = JsonSerializer.Deserialize<List<BackupJob>>(json, options)!;
            var job = jobs[0];

            BackupAlgorithm algorithm;

            switch (job.Method)
            {
                case BackupMethod.Full:
                    algorithm = new FullBackup(job);
                    break;
                case BackupMethod.Differential:
                    algorithm = new DifferentialBackup(job);
                    break;
                case BackupMethod.Incremental:
                    algorithm = new IncrementalBackup(job);
                    break;
                default:
                    Console.WriteLine("Neznámý typ backupu.");
                    return Task.CompletedTask;
            }
            algorithm.Run();

            Console.WriteLine($"[{DateTime.Now}] Backup dokončen");

            return Task.CompletedTask;
        }
    }
}
