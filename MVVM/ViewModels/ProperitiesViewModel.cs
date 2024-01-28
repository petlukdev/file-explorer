using System.IO;
using FileExplorer.MVVM.Bases;
using FileExplorer.MVVM.Converters;
using FileExplorer.MVVM.Models;

namespace FileExplorer.MVVM.ViewModels
{
    public class ProperitiesViewModel : BaseViewModel
    {
        public string ID { get; set; } = "Basic";
        public string Icon { get; set; }
        public string FullName { get; set; } = "Name: ";
        public string Type { get; set; } = "Type: ";
        public string ParentPath { get; set; } = "Path: ";
        public string Size { get; set; } = "Size: ";
        public string DateCreated { get; set; } = "Created: ";
        public string DateModified { get; set; } = "Modified: ";
        public string FileSystem { get; set; } = "File System: ";
        public string Storage { get; set; } = "Storage: ";
        public string UsedStorage { get; set; } = "Used Storage: ";
        public string FreeStorage { get; set; } = "Free Storage: ";

        public ProperitiesViewModel(FileModel fm)
        {
            Icon = fm.Icon;
            FullName += fm.FullName;
            Type += fm.Type.ToString();
            Size += SizeConverter.SizeSuffix(fm.SizeBytes);
            ParentPath += Directory.GetParent(fm.Path);
            DateCreated += fm.DateCreated;
            DateModified += fm.DateModified;
            FileSystem += fm.FileSystem;
            Storage += SizeConverter.SizeSuffix(fm.SizeBytes);
            UsedStorage += SizeConverter.SizeSuffix(fm.UsedSpace);
            FreeStorage += SizeConverter.SizeSuffix(fm.FreeSpace);

            if (fm.IsDrive) { ID = "Drive"; }
            if (fm.IsFolder) { ID = "Folder"; }
        }
    }
}
