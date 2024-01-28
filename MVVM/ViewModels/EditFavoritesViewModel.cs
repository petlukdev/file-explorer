using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FileExplorer.MVVM.Bases;
using FileExplorer.MVVM.Fetchers;
using FileExplorer.MVVM.Models;

namespace FileExplorer.MVVM.ViewModels
{
    public class EditFavoritesViewModel : BaseViewModel
    {
        private ObservableCollection<FavoriteModel> _items = new ObservableCollection<FavoriteModel>();
        public ObservableCollection<FavoriteModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public EditFavoritesViewModel()
        {
            foreach (FavoriteModel item in FavoritesFetcher.FavoritesModelCollection)
                _items.Add(item);
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
        private void Ok(object item)
        {
            if (item == null)
            {
                MessageBox.Show("Item not selected!");
                return;
            }

            FavoritesFetcher.RemoveFromCollections((FavoriteModel)item);

            CloseAction();
        }
    }
}
