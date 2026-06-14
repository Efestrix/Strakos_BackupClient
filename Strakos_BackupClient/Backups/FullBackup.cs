using Strakos_BackupClient.Entities;
using Strakos_BackupClient.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strakos_BackupClient.Backups
{
    public class FullBackup : BackupAlgorithm
    {
        public FullBackup(BackupJob job) : base(job) { }
        public override void Run()
        {
            Console.WriteLine("Spouštím FUll backup");
            foreach (string target in Job.Targets)
            {
                Console.WriteLine($"Target: {target}");
                string backupFolder = CreateBackupFolder(target, "full");

                Console.WriteLine($"Backup folder: {backupFolder}");

                foreach (string source in Job.Sources)
                {
                    Console.WriteLine($"Source: {source}");
                    Copy(source, backupFolder);
                }
                CountRetention(target);
            }
        }
    }
}
