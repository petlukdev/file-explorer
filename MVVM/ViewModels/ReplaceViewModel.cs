using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileExplorer.MVVM.Bases;
using FileExplorer.MVVM.Fetchers;
using FileExplorer.MVVM.Services;
using FileExplorer.MVVM.Helpers;

namespace FileExplorer.MVVM.ViewModels
{
    public class ReplaceViewModel : BaseViewModel
    { 
        private ObservableCollection<TreeViewItem> _items = new ObservableCollection<TreeViewItem>();
        public ObservableCollection<TreeViewItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ReplaceViewModel()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in drives)
                _items.Add(TreeFetcher.CreateTreeItem(driveInfo));
        }

        private ICommand _treeExpandedCommand;
        public ICommand TreeExpandedCommand
        {
            get
            {
                if (_treeExpandedCommand == null)
                    _treeExpandedCommand = new RelayCommand<RoutedEventArgs>(TreeFetcher.TreeExpanded);

                return _treeExpandedCommand;
            }
        }

        private ICommand _stornoCommand;
        public ICommand StornoCommand
        {
            get
            {
                if (_stornoCommand == null)
                    _stornoCommand = new RelayCommand(param => CloseAction());

                return _stornoCommand;
            }
        }

        private ICommand _okCommand;
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new RelayCommand(param => Ok(param));

                return _okCommand;
            }
        }
        public void Ok(object o)
        {
            if (o == null || (o is string && ((string)o).IsEmpty()))
            {
                MessageBox.Show("Destination not selected!");
                return;
            }
            
            if (o is string) { DialogService.Result = o; }

            if (o is TreeViewItem)
            {
                var item = o as TreeViewItem;
                if (item.Tag is DriveInfo) DialogService.Result = ((DriveInfo)item.Tag).Name;
                else DialogService.Result = ((DirectoryInfo)item.Tag).FullName;
            }

            CloseAction();
        }
    }
}
