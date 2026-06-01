using Strakos_BackupClient.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strakos_BackupClient.Helper
{
    public abstract class BackupAlgorithm
    {
        public BackupJob Job;

        protected BackupAlgorithm(BackupJob job)
        {
            Job = job;
        }
        public abstract void Run();
        protected void Copy(string sourceDir, string targetDir)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourceDir);
            DirectoryInfo targetDirectory = new DirectoryInfo(targetDir);

            CopyAll(sourceDirectory, targetDirectory);
        }
        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo file in source.GetFiles())
            {
                Console.WriteLine($"Kopírování {file.FullName} do {target.FullName}"); //kontrola
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }
            foreach (DirectoryInfo subdir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(subdir.Name);
                CopyAll(subdir, nextTargetSubDir);
            }
        }
        protected string CreateBackupFolder(string targetDir, string backupType)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            string time = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFolderPath = Path.Combine(targetDir, $"{backupType}_{time}");

            Directory.CreateDirectory(backupFolderPath);

            return backupFolderPath;
        }
        protected DirectoryInfo GetLastFullBackup(string targetDir)
        {
            DirectoryInfo targetInfo = new DirectoryInfo(targetDir);

            DirectoryInfo? lastFullBackup =
                targetInfo.GetDirectories("full_*")
                          .OrderByDescending(d => d.CreationTime)
                          .FirstOrDefault();

            if (lastFullBackup == null)
                throw new InvalidOperationException("Nenalezen žádný full backup");

            return lastFullBackup;
        }
        protected DirectoryInfo GetLastBackupForIncremental(string targetDir)
        {
            DirectoryInfo targetInfo = new DirectoryInfo(targetDir);

            DirectoryInfo? lastBackup =
                targetInfo.GetDirectories()
                          .Where(d => d.Name.StartsWith("full_") ||
                          d.Name.StartsWith("incr_"))
                          .OrderByDescending(d => d.CreationTime)
                          .FirstOrDefault();

            if (lastBackup == null)
                throw new InvalidOperationException("Nenalezen žádný full nebo incr backup");

            return lastBackup;
        }
        protected FileInfo[] GetAllSourceFiles(string sourceDir)
        {
            DirectoryInfo sourceInfo = new DirectoryInfo(sourceDir);
            return sourceInfo.GetFiles("*", SearchOption.AllDirectories);
        }
        protected string TargetPath(string sourceDir, string targetDir, FileInfo sourceFile)
        {
            string relativePath = Path.GetRelativePath(sourceDir, sourceFile.FullName);
            return Path.Combine(targetDir, relativePath);
        }
        protected void CountRetention(string targetDir)
        {
            if (Job.Retention.Count <= 0)
                return;

            DirectoryInfo target = new DirectoryInfo(targetDir);

            List<DirectoryInfo> backups = target.GetDirectories()
                .OrderBy(d => d.CreationTime)
                .ToList();

            while (backups.Count > Job.Retention.Count)
            {
                Console.WriteLine($"Mazání backupu {backups[0].Name}");
                backups[0].Delete(true);
                backups.RemoveAt(0);
            }
        }
        protected bool SizeRetention(string targetDir)
        {
            if (Job.Retention.Size <= 0)
                return false;

            DirectoryInfo target = new DirectoryInfo(targetDir);
            if (!target.Exists)
                return false;

            List<DirectoryInfo> backups = target.GetDirectories()
                .OrderByDescending(d => d.CreationTime)
                .ToList();

            int length = 0;

            foreach (DirectoryInfo dir in backups)
            {
                if (dir.Name.StartsWith("full_"))
                    break;

                if (dir.Name.StartsWith("diff_") ||
                    dir.Name.StartsWith("incr_"))
                    length++;
            }
            return length >= Job.Retention.Size;
        }
        protected bool HasFullBackup(string targetDir)
        {
            DirectoryInfo target = new DirectoryInfo(targetDir);

            return target.Exists && target.GetDirectories()
                .Any(d => d.Name.StartsWith("full_"));
        }
    }
}
