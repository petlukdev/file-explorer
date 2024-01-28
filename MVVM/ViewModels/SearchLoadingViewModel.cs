using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FileExplorer.MVVM.Helpers;
using FileExplorer.MVVM.Bases;
using FileExplorer.MVVM.Enums;
using FileExplorer.MVVM.Services;
using FileExplorer.MVVM.Models;

namespace FileExplorer.MVVM.ViewModels
{
    public class SearchLoadingViewModel : BaseViewModel
    {
        private CancellationTokenSource _tokenSource;
        
        private int _searchProgress = 0;
        public int SearchProgress
        {
            get => _searchProgress;
            set
            {
                _searchProgress = value;
                OnPropertyChanged(nameof(SearchProgress));
            }
        }

        public SearchLoadingViewModel(string dir, string patt)
        {
            Search(dir, patt);
        }

        private async void Search(string dir, string patt)
        {
            _tokenSource = new CancellationTokenSource();
            
            await SearchAsync(dir, patt, _tokenSource.Token);
            CloseAction();
        }

        private async Task SearchAsync(string directory, string searchPattern, CancellationToken token)
        {
            var result = new ObservableCollection<FileModel>();
            var worker = new BackgroundWorker();

            worker.WorkerReportsProgress = true;
            worker.DoWork += (o, e) =>
            {
                int total = 0;
                int max = 5000;

                try 
                {
                    foreach (var item in Directory.EnumerateDirectories(directory, searchPattern, SearchOption.AllDirectories)) { total++; }
                } catch { }

                try
                {
                    foreach (var item in Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories)) { total++; }
                } catch { }

                int current = 0;
                DirectoryInfo dInfo;
                FileModel fm;

                try
                {
                    foreach (var dir in Directory.EnumerateDirectories(directory, searchPattern, SearchOption.AllDirectories))
                    {
                        if (token.IsCancellationRequested) return;

                        dInfo = new DirectoryInfo(dir);

                        if (dInfo.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                        fm = new FileModel
                        {
                            Name = dInfo.Name,
                            FullName = dInfo.Name,
                            Path = dInfo.FullName,
                            DateCreated = dInfo.CreationTime,
                            DateModified = dInfo.LastWriteTime,
                            Type = FileType.Folder
                        };

                        if (result.Count > max)
                        {
                            MessageBox.Show("Directory contains too many objects with this parameter. For technical reasons, searching was aborted!");
                            return;
                        }

                        result.Add(fm);
                        current++;
                        worker.ReportProgress((current * 100) / total);
                    }
                }
                catch { }

                FileInfo fInfo;

                try
                {
                    foreach (var file in Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories))
                    {
                        if (token.IsCancellationRequested) return;

                        fInfo = new FileInfo(file);

                        if (fInfo.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                        fm = new FileModel
                        {
                            Name = fInfo.FullName.GetFileNameWithoutExtension(),
                            FullName = fInfo.Name,
                            Path = fInfo.FullName,
                            DateCreated = fInfo.CreationTime,
                            DateModified = fInfo.LastWriteTime,
                            Type = FileType.File,
                            SizeBytes = fInfo.Length
                        };

                        if (result.Count > max)
                        {
                            MessageBox.Show("Directory contains too many objects with this parameter. For technical reasons, searching was aborted!");
                            _tokenSource.Cancel();
                            return;
                        }

                        result.Add(fm);
                        current++;
                        worker.ReportProgress((current * 100) / total);
                    }
                }
                catch { }

                DialogService.Result = result;
            };
            worker.ProgressChanged += (o, e) => { SearchProgress = e.ProgressPercentage; };

            await Task.Run(() =>
            {
                worker.RunWorkerAsync();
                while (worker.IsBusy)
                {
                    if (token.IsCancellationRequested) return;
                    Task.Delay(100);
                }
            }, token);
        }

        private ICommand _stornoCommand;
        public ICommand StornoCommand
        {
            get
            {
                if (_stornoCommand == null)
                    _stornoCommand = new RelayCommand(param => Storno());

                return _stornoCommand;
            }
        }
        private void Storno()
        {
            _tokenSource.Cancel();

            CloseAction();
        }
    }
}
