using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.MVVM.Fetchers
{
    public static class TreeFetcher
    {
        public static void TreeExpanded(RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();

                DirectoryInfo expandedDir = null;
                if (item.Tag is DriveInfo)
                    expandedDir = (item.Tag as DriveInfo).RootDirectory;
                if (item.Tag is DirectoryInfo)
                    expandedDir = (item.Tag as DirectoryInfo);

                try
                {
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                    {
                        if (subDir.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                        item.Items.Add(CreateTreeItem(subDir));
                    }
                } catch { }
            }
        }

        public static TreeViewItem CreateTreeItem(object o)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = o.ToString(),
                Tag = o
            };
            item.Items.Add("If you see this, something went wrong!");

            return item;
        }
    }
}
