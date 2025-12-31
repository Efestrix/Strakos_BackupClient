using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strakos_BackupClient
{
    public class IncrementalBackup : BackupAlgorithm
    {
        public IncrementalBackup(BackupJob job) : base(job) { }
        public override void Run()
        {
            foreach (var target in Job.Targets)
            {
                if (!Directory.Exists(target))
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

                DirectoryInfo lastBackup = GetLastBackupForIncremental(target);

                string incFolder = CreateBackupFolder(target, "incr");
                foreach (var source in Job.Sources)
                {
                    if (!Directory.Exists(source))
                        continue;

                    foreach (FileInfo sourceFile in GetAllSourceFiles(source))
                    {
                        string lastBackupFilePath = TargetPath(source, lastBackup.FullName, sourceFile);

                        string targetFilePath = TargetPath(source, incFolder, sourceFile);

                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                        if (!File.Exists(lastBackupFilePath))
                        {
                            sourceFile.CopyTo(targetFilePath, true);
                        }
                        else
                        {
                            DateTime fullTime = File.GetLastWriteTime(lastBackupFilePath);

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
