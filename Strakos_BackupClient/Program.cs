using Quartz;
using Quartz.Impl;
using System.Text.Json;

namespace Strakos_BackupClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Startuji BackupClient");

            if (!File.Exists("Config.json"))
            {
                Console.WriteLine("Config.json nebyl nalezen");
                return;
            }

            string json = File.ReadAllText("Config.json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            List<BackupJob> jobs = JsonSerializer.Deserialize<List<BackupJob>>(json, options)!;

            if (jobs == null || jobs.Count == 0)
            {
                Console.WriteLine("Config.json neobsahuje žádný backup job.");
                return;
            }

            BackupJob job = jobs[0];

            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            IJobDetail jobDetail = JobBuilder
                .Create<QuartzJob>()
                .Build();

            ITrigger trigger = TriggerBuilder
                .Create()
                .WithIdentity("BackupTrigger")
                .WithCronSchedule(job.Timing)
                .StartNow()
                .Build();

            await scheduler.Start();
            await scheduler.ScheduleJob(jobDetail, trigger);

            Console.WriteLine("Backup dokončen.");
            await Task.Delay(-1);
        }
    }
}
