using Quartz;
using Quartz.Impl;
using Strakos_BackupClient.Api;
using Strakos_BackupClient.Entities;
using Strakos_BackupClient.Timing;
using System.Text.Json;

namespace Strakos_BackupClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Startuji BackupClient");

            string computerUuid = "a3543586-4995-46d2-a27f-07b5e2e98fe7";
            string apiUrl = "https://localhost:7273";

            ApiConfigRepository repository = new ApiConfigRepository(apiUrl);
            List<BackupJob> jobs = await repository.LoadJobsAsync(computerUuid);

            if (jobs == null || jobs.Count == 0)
            {
                Console.WriteLine("API nevrátilo žádný backup job.");
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                BackupJob job = jobs[i];

                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();

                IJobDetail jobDetail = JobBuilder
                    .Create<QuartzJob>()
                    .UsingJobData("computerUuid", computerUuid)
                    .UsingJobData("apiUrl", apiUrl)
                    .UsingJobData("jobIndex", job.Id)
                    .WithIdentity($"Job_{job.Id}")
                    .Build();

                string quartzCron = ConvertToQuartzCron(job.Timing);

                Console.WriteLine("Quartz Cron: " + quartzCron);

                ITrigger trigger = TriggerBuilder
                    .Create()
                    .WithIdentity($"Trigger_{job.Id}")
                    .WithCronSchedule(quartzCron)
                    .StartNow()
                    .Build();

                await scheduler.Start();
                await scheduler.ScheduleJob(jobDetail, trigger);
            }

            await Task.Delay(-1);
        }
        private static string ConvertToQuartzCron(string cron)
        {
            string[] parts = cron.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 5)
                throw new Exception("Timing musí mít 5 částí: minuta hodina den měsíc denVTýdnu");

            string minute = parts[0];
            string hour = parts[1];
            string dayOfMonth = parts[2];
            string month = parts[3];

            return $"0 {minute} {hour} {dayOfMonth} {month} ?";
        }
    }
}
