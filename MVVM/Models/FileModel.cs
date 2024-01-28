using System;
using FileExplorer.MVVM.Enums;

namespace FileExplorer.MVVM.Models
{
    public class FileModel
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public long SizeBytes { get; set; }
        public long UsedSpace { get; set; }
        public long FreeSpace { get; set; }
        public string FileSystem { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public FileType Type { get; set; }
        public bool IsFile => Type == FileType.File;
        public bool IsFolder => Type == FileType.Folder;
        public bool IsDrive => Type == FileType.Drive;
        public string Icon
        {
            get
            {
                if (IsFolder) return "pack://application:,,,/Images/folder.png";

                if (IsDrive) return "pack://application:,,,/Images/drive.png";

                return "pack://application:,,,/Images/file.png";
            }
        }
    }
}
