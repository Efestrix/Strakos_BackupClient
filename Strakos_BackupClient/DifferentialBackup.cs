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
            if (!ExistsFullBackup())
            {
                Console.WriteLine("Nelze provést differential backup");
                Console.WriteLine("Neexistuje full backup");
                return;
            }
            foreach (var target in Job.Targets)
            {
                DirectoryInfo lastFullBackup = GetLastFullBackup(target);

                string diffFolder = CreateBackupFolder(target, "diff");
                foreach (var source in Job.Sources)
                {
                    FileInfo[] sourceFiles = GetAllSourceFiles(source);

                    foreach (FileInfo sourceFile in sourceFiles)
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
                ApplyRetention(target);
            }
        }
        protected bool ExistsFullBackup()
        {
            foreach (var target in Job.Targets)
            {
                DirectoryInfo fullBackup = new DirectoryInfo(target);

                if (fullBackup.GetDirectories().Any(dir => dir.Name.StartsWith("full_")))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
