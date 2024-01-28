using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Linq;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using FileExplorer.MVVM.Models;
using FileExplorer.MVVM.Views;
using FileExplorer.MVVM.Fetchers;
using FileExplorer.MVVM.Helpers;
using FileExplorer.MVVM.Bases;
using FileExplorer.MVVM.Services;
using FileExplorer.MVVM.Enums;

namespace FileExplorer.MVVM.ViewModels
{
    public class ExplorerViewModel : BaseViewModel
    {
        private ObservableCollection<FileModel> _directoryCollection = new ObservableCollection<FileModel>();
        public ObservableCollection<FileModel> DirectoryCollection
        {
            get => _directoryCollection;
            set
            {
                _directoryCollection = value;
                OnPropertyChanged(nameof(DirectoryCollection));
            }
        }

        private ObservableCollection<TreeViewItem> _treeCollection = new ObservableCollection<TreeViewItem>();
        public ObservableCollection<TreeViewItem> TreeCollection
        {
            get => _treeCollection;
            set
            {
                _treeCollection = value;
                OnPropertyChanged(nameof(TreeCollection));
            }
        }

        private ObservableCollection<TreeViewItem> _favoritesCollection = new ObservableCollection<TreeViewItem>();
        public ObservableCollection<TreeViewItem> FavoritesCollection
        {
            get => _favoritesCollection;
            set
            {
                _favoritesCollection = value;
                OnPropertyChanged(nameof(FavoritesCollection));
            }
        }

        private string _myPC = "My Computer";
        private string _searchResults = "Search Results";

        private string _currentDirectory;
        public string CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                _currentDirectory = value;
                OnPropertyChanged(nameof(CurrentDirectory));
            }
        }

        public ExplorerViewModel()
        {
            _currentDirectory = _myPC;
            
            FavoritesFetcher.SetupFavorites();
            NavigationService.AddPage(_currentDirectory);
            PopulateBrowser(_currentDirectory);

            foreach (TreeViewItem item in FavoritesFetcher.FavoritesTreeCollection)
                _favoritesCollection.Add(item);
            
            foreach (DriveInfo drInfo in DriveInfo.GetDrives().Where(d => d.IsReady))
                _treeCollection.Add(TreeFetcher.CreateTreeItem(drInfo));
        }

        private void PopulateBrowser(string directory)
        {
            _directoryCollection.Clear();

            if (directory == _myPC) DirectoryFetcher.GetDrives().ForEach(dr => _directoryCollection.Add(dr));
            else
            {
                DirectoryFetcher.GetDirectories(directory).ForEach(d => _directoryCollection.Add(d));
                DirectoryFetcher.GetFiles(directory).ForEach(f => _directoryCollection.Add(f));
            }
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

        private ICommand _treeSelectedCommand;
        public ICommand TreeSelectedCommand
        {
            get
            {
                if (_treeSelectedCommand == null)
                    _treeSelectedCommand = new RelayCommand<RoutedEventArgs>(TreeSelected);

                return _treeSelectedCommand;
            }
        }
        private void TreeSelected(RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;

            FileModel fm = new FileModel
            {
                Type = FileType.Folder
            };

            if (item.Tag is DriveInfo)
            {
                DirectoryInfo expandedDir = (item.Tag as DriveInfo).RootDirectory;
                fm.Path = expandedDir.FullName;
            }

            if (item.Tag is DirectoryInfo)
                fm.Path = (item.Tag as DirectoryInfo).FullName;
            if (item.Tag is string)
                fm.Path = item.Tag as string;

            OpenItem(fm);
        }

        private ICommand _addFavoriteCommand;
        public ICommand AddFavoriteCommand
        {
            get
            {
                if (_addFavoriteCommand == null)
                    _addFavoriteCommand = new RelayCommand(param => AddFavorite((FileModel)param));

                return _addFavoriteCommand;
            }
        }
        private void AddFavorite(FileModel fm)
        {
            if (_favoritesCollection.Any(p => (string)p.Tag == fm.Path))
            {
                MessageBox.Show("This directory is already in favorites!");
                return;
            }

            if (fm.IsFile)
            {
                MessageBox.Show("You cannot add files to favorites!");
                return;
            }

            TreeViewItem newItem = new TreeViewItem
            {
                Header = fm.Name,
                Tag = fm.Path
            };

            FavoritesFetcher.AddToCollections(newItem);
            _favoritesCollection.Add(newItem);
            OnPropertyChanged(nameof(FavoritesCollection));
        }

        private ICommand _editFavoriteCommand;
        public ICommand EditFavoriteCommand
        {
            get
            {
                if (_editFavoriteCommand == null)
                    _editFavoriteCommand = new RelayCommand(param => EditFavorites());

                return _editFavoriteCommand;
            }
        }
        private void EditFavorites()
        {
            DialogService.ShowDialog<EditFavoritesView>(new EditFavoritesViewModel());

            _favoritesCollection.Clear();
            foreach (TreeViewItem item in FavoritesFetcher.FavoritesTreeCollection)
                _favoritesCollection.Add(item);

            OnPropertyChanged(nameof(FavoritesCollection));
        }

        private ICommand _openCommand;
        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                    _openCommand = new RelayCommand(param => OpenItem((FileModel)param));

                return _openCommand;
            }
        }
        private void OpenItem(FileModel fm)
        {
            if (!fm.IsFile)
            {
                _currentDirectory = fm.Path;
                OnPropertyChanged(nameof(CurrentDirectory));

                NavigationService.AddPage(_currentDirectory);
                PopulateBrowser(_currentDirectory);
                return;
            }

            try { Process.Start(fm.Path); }
            catch (UnauthorizedAccessException e) { MessageBox.Show(e.Message); }
        }

        private ICommand _returnCommand;
        public ICommand ReturnCommand
        {
            get
            {
                if (_returnCommand == null)
                    _returnCommand = new RelayCommand(param => MoveToPreviousDirectory());

                return _returnCommand;
            }
        }
        private void MoveToPreviousDirectory()
        {
            _currentDirectory = NavigationService.GetPreviousPage();
            OnPropertyChanged(nameof(CurrentDirectory));

            PopulateBrowser(_currentDirectory);
        }

        private ICommand _forwardCommand;
        public ICommand ForwardCommand
        {
            get
            {
                if (_forwardCommand == null)
                    _forwardCommand = new RelayCommand(param => MoveToForwardDirectory());

                return _forwardCommand;
            }
        }
        private void MoveToForwardDirectory()
        {
            _currentDirectory = NavigationService.GetNextPage();
            OnPropertyChanged(nameof(CurrentDirectory));

            PopulateBrowser(_currentDirectory);
        }

        private ICommand _newFileCommand;
        public ICommand NewFileCommand
        {
            get
            {
                if (_newFileCommand == null)
                    _newFileCommand = new RelayCommand(param => CreateFile());

                return _newFileCommand;
            }
        }
        private void CreateFile()
        {
            try
            {
                if (_currentDirectory == _myPC || _currentDirectory == _searchResults)
                {
                    MessageBox.Show("You cannot make files or folders in this directory!");
                    return;
                }

                int num = 1;
                string newFile = $"New_File{num}.txt";
                string path = Path.Combine(_currentDirectory, newFile);
                while (File.Exists(path))
                {
                    num++;
                    newFile = $"New_File{num}.txt";
                    path = Path.Combine(_currentDirectory, newFile);
                }
                File.Create(path).Dispose();

                FileModel fm = new FileModel
                {
                    Name = path.GetFileNameWithoutExtension(),
                    FullName = path.GetFileName(),
                    Path = path,
                    Type = FileType.File,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    SizeBytes = new FileInfo(path).Length
                };
                _directoryCollection.Add(fm);
                OnPropertyChanged(nameof(DirectoryCollection));
            }
            catch (UnauthorizedAccessException e) { MessageBox.Show(e.Message); }
        }

        private ICommand _newFolderCommand;
        public ICommand NewFolderCommand
        {
            get
            {
                if (_newFolderCommand == null)
                    _newFolderCommand = new RelayCommand(param => CreateFolder());

                return _newFolderCommand;
            }
        }
        private void CreateFolder()
        {
            try
            {
                if (_currentDirectory == _myPC || _currentDirectory == _searchResults)
                {
                    MessageBox.Show("You cannot make files or folders in this directory!");
                    return;
                }

                int num = 1;
                string newFolder = $"New_Folder{num}";
                string path = Path.Combine(_currentDirectory, newFolder);
                while (Directory.Exists(path))
                {
                    num++;
                    newFolder = $"New_Folder{num}";
                    path = Path.Combine(_currentDirectory, newFolder);
                }
                Directory.CreateDirectory(path);

                FileModel fm = new FileModel
                {
                    Name = path.GetFileNameWithoutExtension(),
                    Path = path,
                    Type = FileType.Folder,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                };
                _directoryCollection.Add(fm);
                OnPropertyChanged(nameof(DirectoryCollection));
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(param => DeleteItem((FileModel)param));

                return _deleteCommand;
            }
        }
        private void DeleteItem(FileModel item)
        {
            try
            {
                if (item == null)
                {
                    MessageBox.Show("Item not selected!");
                    return;
                }
                
                if (item.IsDrive)
                {
                    MessageBox.Show("You cannot delete drives!");
                    return;
                }

                if (item.IsFile)
                    FileSystem.DeleteFile(item.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
                else
                    FileSystem.DeleteDirectory(item.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);

                _directoryCollection.Remove(item);
                OnPropertyChanged(nameof(DirectoryCollection));
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private ICommand _renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                if (_renameCommand == null)
                    _renameCommand = new RelayCommand(param => RenameItem((FileModel)param));

                return _renameCommand;
            }
        }
        private void RenameItem(FileModel item)
        {
            try
            {
                if (item == null)
                {
                    MessageBox.Show("Item not selected!");
                    return;
                }
                
                if (item.IsDrive)
                {
                    MessageBox.Show("You cannot rename a drive!");
                    return;
                }

                string input = Microsoft.VisualBasic.Interaction.InputBox("Insert new name of file or directory.");
                string newPath = item.Path.Replace(item.Name, input);
                FileType type = FileType.File;
                long fmSize = 0;

                if (input.IsEmpty()) return;

                if (item.IsFile)
                {
                    File.Move(item.Path, newPath);
                    fmSize = new FileInfo(newPath).Length;
                }

                if (item.IsFolder)
                {
                    Directory.Move(item.Path, newPath);
                    type = FileType.Folder;
                }

                _directoryCollection.Remove(item);
                FileModel newItem = new FileModel
                {
                    Name = input,
                    FullName = newPath.GetFileName(),
                    Path = newPath,
                    DateCreated = item.DateCreated,
                    DateModified = DateTime.Now,
                    Type = type,
                    SizeBytes = fmSize
                };
                _directoryCollection.Add(newItem);
                OnPropertyChanged(nameof(DirectoryCollection));
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private ICommand _showProperitiesCommand;
        public ICommand ShowProperitiesCommand
        {
            get
            {
                if (_showProperitiesCommand == null)
                    _showProperitiesCommand = new RelayCommand(param => ShowProps((FileModel)param));

                return _showProperitiesCommand;
            }
        }
        private void ShowProps(FileModel fm)
        {
            if (fm == null)
            {
                MessageBox.Show("Item not selected!");
                return;
            }
            
            DialogService.Show<ProperitiesView>(new ProperitiesViewModel(fm));
        }

        private ICommand _copyCommand;
        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                    _copyCommand = new RelayCommand(param => CopyItem((FileModel)param));

                return _copyCommand;
            }
        }
        private void CopyItem(FileModel item)
        {
            try
            {
                if (item == null)
                {
                    MessageBox.Show("Item not selected!");
                    return;
                }
                
                StringCollection path = new StringCollection { item.Path };
                Clipboard.SetFileDropList(path);
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private ICommand _pasteCommand;
        public ICommand PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                    _pasteCommand = new RelayCommand(param => PasteItem());

                return _pasteCommand;
            }
        }

        private void PasteItem()
        {
            try
            {
                if (_currentDirectory == _myPC || _currentDirectory == _searchResults)
                {
                    MessageBox.Show("You cannot paste files or folders in this directory!");
                    return;
                }

                StringCollection path = Clipboard.GetFileDropList();

                string name = path[0].GetFileNameWithoutExtension();
                string extension = Path.GetExtension(path[0]);
                int num = 1;
                string test = $"{name} - Copy{num}{extension}";
                string newPath = Path.Combine(_currentDirectory, test);

                FileType type = FileType.File;

                if (path[0].IsFile())
                {
                    while (newPath.IsFile())
                    {
                        num++;
                        test = $"{name} - Copy{num}{extension}";
                        newPath = Path.Combine(_currentDirectory, test);
                    }
                    File.Copy(path[0], newPath);
                }

                if (path[0].IsDirectory())
                {
                    while (newPath.IsDirectory())
                    {
                        num++;
                        test = $"{name} - Copy{num}{extension}";
                        newPath = Path.Combine(_currentDirectory, test);
                    }
                    DirectoryFetcher.CopyDirectory(path[0], newPath, true);
                    type = FileType.Folder;
                }

                FileModel fm = new FileModel
                {
                    Name = newPath.GetFileNameWithoutExtension(),
                    FullName = newPath.GetFileName(),
                    Path = newPath,
                    Type = type,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                };

                _directoryCollection.Add(fm);
                OnPropertyChanged(nameof(DirectoryCollection));
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private ICommand _moveCommand;
        public ICommand MoveCommand
        {
            get
            {
                if (_moveCommand == null)
                    _moveCommand = new RelayCommand(param => MoveItem((FileModel)param));

                return _moveCommand;
            }
        }
        private void MoveItem(FileModel fm)
        {
            if (fm == null)
            {
                MessageBox.Show("Item not selected!");
                return;
            }

            if (fm.IsDrive)
            {
                MessageBox.Show("You cannot move drives!");
                return;
            }

            DialogService.ShowDialog<ReplaceView>(new ReplaceViewModel());
            string path = fm.Path;
            string dialogResult = DialogService.Result as string;
            if (dialogResult == null) return;
            string newPath = Path.Combine(dialogResult, fm.FullName);

            try
            {
                if (newPath.IsDirectory() || newPath.IsFile())
                {
                    MessageBoxResult result = MessageBox.Show("An item in the destination already exists. Would you like to replace it?", "test", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                    {
                        MessageBox.Show("An operation was aborted!");
                        return;
                    }

                    if (newPath.IsFile()) File.Delete(newPath);
                    else Directory.Delete(newPath, true);
                }

                if (Path.GetPathRoot(path) != Path.GetPathRoot(newPath))
                {
                    if (path.IsDirectory())
                    {
                        DirectoryFetcher.CopyDirectory(path, newPath, true);
                        Directory.Delete(path, true);
                    }
                    else
                    {
                        File.Copy(path, newPath);
                        File.Delete(path);
                    }
                }
                else Directory.Move(path, newPath);

                _directoryCollection.Remove(fm);
                OnPropertyChanged(nameof(DirectoryCollection));
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private ICommand _zipCommand;
        public ICommand ZipCommand
        {
            get
            {
                if (_zipCommand == null)
                    _zipCommand = new RelayCommand(param => Zip((FileModel)param));

                return _zipCommand;
            }
        }
        private void Zip(FileModel fm)
        {
            try
            {
                if (fm == null)
                {
                    MessageBox.Show("Item not selected!");
                    return;
                }

                if (fm.IsDrive)
                {
                    MessageBox.Show("You cannot archive a drive!");
                    return;
                }

                int num = 1;
                string name = $"{fm.Name} - Zip{num}.zip";
                string newPath = Path.Combine(_currentDirectory, name);
                while (newPath.IsFile())
                {
                    num++;
                    name = $"{fm.Name} - Zip{num}.zip";
                    newPath = Path.Combine(_currentDirectory, name);
                }
                
                if (fm.IsFile)
                {
                    ZipArchive zipArch = ZipFile.Open(newPath, ZipArchiveMode.Create);
                    string temp = fm.Path.GetFileName();
                    zipArch.CreateEntryFromFile(fm.Path, temp);
                    zipArch.Dispose();
                }

                if (fm.IsFolder) ZipFile.CreateFromDirectory(fm.Path, newPath);

                FileModel newFm = new FileModel
                {
                    Name = newPath.GetFileNameWithoutExtension(),
                    FullName = newPath.GetFileName(),
                    Path = newPath,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                };

                _directoryCollection.Add(newFm);
                OnPropertyChanged(nameof(DirectoryCollection));
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private ICommand _unZipCommand;
        public ICommand UnZipCommand
        {
            get
            {
                if (_unZipCommand == null)
                    _unZipCommand = new RelayCommand(param => UnZip((FileModel)param));

                return _unZipCommand;
            }
        }
        private void UnZip(FileModel fm)
        {
            int num = 1;
            string name = $"{fm.Name} - Extracted{num}";
            string newPath = Path.Combine(_currentDirectory, name);
            while (newPath.IsDirectory())
            {
                num++;
                name = $"{fm.Name} - Extracted{num}";
                newPath = Path.Combine(_currentDirectory, name);
            }

            ZipFile.ExtractToDirectory(fm.Path, newPath);

            FileModel newFm = new FileModel
            {
                Name = newPath.GetFileNameWithoutExtension(),
                FullName = newPath.GetFileName(),
                Path= newPath,
                DateCreated= DateTime.Now,
                DateModified= DateTime.Now,
                Type = FileType.Folder
            };

            _directoryCollection.Add(newFm);
            OnPropertyChanged(nameof(DirectoryCollection));
        }

        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand(param => Search((string)param));

                return _searchCommand;
            }
        }
        private void Search(string param)
        {
            if (_currentDirectory == _myPC || _currentDirectory == _searchResults)
            {
                MessageBox.Show("You cannot search from this directory!");
                return;
            }

            if (param.IsEmpty())
            {
                MessageBox.Show("Parameter not detected!");
                return;
            }

            DialogService.ShowDialog<SearchLoadingView>(new SearchLoadingViewModel(_currentDirectory, param));

            if (!(DialogService.Result is ObservableCollection<FileModel>) || DialogService.Result == null) return;

            NavigationService.AddPage(_currentDirectory);
            
            _currentDirectory = _searchResults;
            OnPropertyChanged(nameof(CurrentDirectory));

            _directoryCollection = (ObservableCollection<FileModel>)DialogService.Result;
            OnPropertyChanged(nameof(DirectoryCollection));
        }
    }
}
