using Quartz;
using Strakos_BackupClient.Api;
using Strakos_BackupClient.Backups;
using Strakos_BackupClient.Entities;
using Strakos_BackupClient.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Strakos_BackupClient.Timing
{
    public class QuartzJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            string computerUuid = context.JobDetail.JobDataMap.GetString("computerUuid")!;
            string apiUrl = context.JobDetail.JobDataMap.GetString("apiUrl")!;

            int jobId = context.JobDetail.JobDataMap.GetInt("jobId");

            ApiConfigRepository repository = new ApiConfigRepository(apiUrl);
            List<BackupJob> jobs = await repository.LoadJobsAsync(computerUuid);

            BackupJob? job = jobs
                .FirstOrDefault(j => j.Id == jobId);

            if (jobs == null || jobs.Count == 0)
            {
                Console.WriteLine($"Job ID {jobId} nebyl nalezen");
                return;
            }

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
                    return;
            }
            algorithm.Run();

            Console.WriteLine($"[{DateTime.Now}] Backup dokončen");
        }
    }
}
