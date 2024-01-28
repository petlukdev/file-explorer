using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileExplorer.MVVM.Enums;
using FileExplorer.MVVM.Helpers;
using FileExplorer.MVVM.Models;

namespace FileExplorer.MVVM.Fetchers
{
    public static class DirectoryFetcher
    {
        public static List<FileModel> GetFiles(string directory)
        {
            List<FileModel> files = new List<FileModel>();

            if (!directory.IsDirectory()) return files;

            try
            {
                FileInfo fInfo;

                foreach (string file in Directory.GetFiles(directory))
                {
                    fInfo = new FileInfo(file);

                    if (fInfo.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                    FileModel fModel = new FileModel()
                    {
                        Name = fInfo.FullName.GetFileNameWithoutExtension(),
                        FullName = fInfo.Name,
                        Path = fInfo.FullName,
                        DateCreated = fInfo.CreationTime,
                        DateModified = fInfo.LastWriteTime,
                        Type = FileType.File,
                        SizeBytes = fInfo.Length
                    };

                    files.Add(fModel);
                }

            } catch { }

            return files;
        }

        public static List<FileModel> GetDirectories(string directory)
        {
            List<FileModel> directories = new List<FileModel>();

            if (!directory.IsDirectory()) return directories;

            try
            {
                DirectoryInfo dInfo;

                foreach (string dir in Directory.GetDirectories(directory))
                {
                    dInfo = new DirectoryInfo(dir);

                    if (dInfo.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                    FileModel dModel = new FileModel()
                    {
                        Name = dInfo.Name,
                        FullName = dInfo.Name,
                        Path = dInfo.FullName,
                        DateCreated = dInfo.CreationTime,
                        DateModified = dInfo.LastWriteTime,
                        Type = FileType.Folder,
                    };

                    directories.Add(dModel);
                }

            } catch { }

            return directories;
        }

        public static List<FileModel> GetDrives()
        {
            List<FileModel> drives = new List<FileModel>();

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                {
                    FileModel dModel = new FileModel()
                    {
                        Name = drive.Name,
                        FullName = drive.Name,
                        Path = drive.Name,
                        Type = FileType.Drive,
                        SizeBytes = drive.TotalSize,
                        UsedSpace = drive.TotalSize - drive.AvailableFreeSpace,
                        FreeSpace = drive.AvailableFreeSpace,
                        FileSystem = drive.DriveFormat
                    };

                    drives.Add(dModel);
                }

            } catch { }

            return drives;
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            if (!recursive) return;

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
