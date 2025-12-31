using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strakos_BackupClient
{
    public class DifferentialBackup : BackupAlgorithm
    {
        public DifferentialBackup(BackupJob job) : base(job) { }
        public override void Run()
        {
            foreach (var target in Job.Targets)
            {
                if(!Directory.Exists(target))
                {
                    Console.WriteLine($"Target neexistuje: {target}");
                    continue;
                }
                if (!HasFullBackup(target))
                {
                    Console.WriteLine($"Target {target} nemá full backup");
                    continue;
                }
                if (SizeRetention(target))
                {
                    Console.WriteLine("Limit size dosažen");
                    continue;
                }

                DirectoryInfo lastFullBackup = GetLastFullBackup(target);
                string diffFolder = CreateBackupFolder(target, "diff");

                foreach (var source in Job.Sources)
                {
                    if (!Directory.Exists(source))
                    {
                        Console.WriteLine($"Source neexistuje: {source}");
                        continue;
                    }

                    foreach (FileInfo sourceFile in GetAllSourceFiles(source))
                    {
                        string fullBackupFilePath = TargetPath(source, lastFullBackup.FullName, sourceFile);

                        string targetFilePath = TargetPath(source, diffFolder, sourceFile);

                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                        if (!File.Exists(fullBackupFilePath))
                        {
                            sourceFile.CopyTo(targetFilePath, true);
                        }
                        else
                        {
                            DateTime fullTime = File.GetLastWriteTime(fullBackupFilePath);

                            if (sourceFile.LastWriteTime > fullTime)
                            {
                                sourceFile.CopyTo(targetFilePath, true);
                            }
                        }
                    }
                }
                CountRetention(target);
            }
        }
    }
}
