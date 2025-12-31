using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strakos_BackupClient
{
    public class FullBackup : BackupAlgorithm
    {
        public FullBackup(BackupJob job) : base(job) { }
        public override void Run()
        {
            foreach (var target in Job.Targets)
            {
                string backupFolder = CreateBackupFolder(target, "full");
                foreach (var source in Job.Sources)
                {
                    Copy(source, backupFolder);
                }
                CountRetention(target);
            }
        }
    }
}
