using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strakos_BackupClient
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
        protected void ApplyRetention(string targetDir)
        {
            CountRetention(targetDir);
            SizeRetention(targetDir);
        }
        private void CountRetention(string targetDir)
        {
            if (Job.Retention.Count <= 0)
                return;

            DirectoryInfo target = new DirectoryInfo(targetDir);

            List<DirectoryInfo> backups = target.GetDirectories()
                .OrderBy(d => d.CreationTime)
                .ToList();

            while (backups.Count > Job.Retention.Count)
            {
                backups[0].Delete(true);
                backups.RemoveAt(0);
            }
        }
        private void SizeRetention(string targetDir)
        {
            if (Job.Retention.Size <= 0)
                return;

            DirectoryInfo target = new DirectoryInfo(targetDir);

            List<DirectoryInfo> backups = target.GetDirectories()
                .OrderBy(d => d.CreationTime)
                .ToList();

            long totalSize = backups.Sum(b => GetDirectorySize(b));

            while (totalSize > Job.Retention.Size && backups.Count > 0)
            {
                long removedSize = GetDirectorySize(backups[0]);
                backups[0].Delete(true);
                backups.RemoveAt(0);
                totalSize -= removedSize;
            }
        }
        protected long GetDirectorySize(DirectoryInfo dir)
        {
            return dir.GetFiles("*", SearchOption.AllDirectories)
                       .Sum(f => f.Length);
        }
    }
}
