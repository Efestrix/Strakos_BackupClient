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
            if (!ExistsAnyBackup())
            {
                Console.WriteLine("Nelze provést incremental backup");
                Console.WriteLine("Neexistuje full, diff ani incr backup");
                return;
            }
            foreach (var target in Job.Targets)
            {
                DirectoryInfo lastBackup = GetLastBackupForIncremental(target);

                string incFolder = CreateBackupFolder(target, "incr");
                foreach (var source in Job.Sources)
                {
                    FileInfo[] sourceFiles = GetAllSourceFiles(source);

                    foreach (FileInfo sourceFile in sourceFiles)
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
                ApplyRetention(target);
            }
        }
        protected bool ExistsAnyBackup()
        {
            foreach (var target in Job.Targets)
            {
                DirectoryInfo backup = new DirectoryInfo(target);

                if (backup.GetDirectories().Any(dir => dir.Name.StartsWith("full_") ||
                dir.Name.StartsWith("diff_") ||
                dir.Name.StartsWith("incr_")))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
