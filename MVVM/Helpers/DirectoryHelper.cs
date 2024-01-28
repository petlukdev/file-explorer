using System.IO;

namespace FileExplorer.MVVM.Helpers
{
    public static class DirectoryHelper
    {
        public static bool IsEmpty(this string txt) { return string.IsNullOrEmpty(txt); }
        
        public static bool IsFile(this string path) { return !string.IsNullOrEmpty(path) && File.Exists(path); }

        public static bool IsDirectory(this string path) { return !string.IsNullOrEmpty(path) && Directory.Exists(path); }

        public static bool IsDrive(this string path) { return Path.GetPathRoot(path) == path; }

        public static string GetFileNameWithoutExtension(this string path) { return Path.GetFileNameWithoutExtension(path); }

        public static string GetFileName(this string path) { return Path.GetFileName(path); }
    }
}
