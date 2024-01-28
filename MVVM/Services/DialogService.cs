using System;
using System.Windows;
using FileExplorer.MVVM.Bases;

namespace FileExplorer.MVVM.Services
{
    public static class DialogService
    {
        public static object Result { get; set; }

        public static void Show<T>(object vm) where T : Window, new()
        {
            T view = new T
            {
                DataContext = vm
            };

            view.Show();
        }

        public static void ShowDialog<T>(object vm) where T : Window, new()
        {
            var viewModel = vm as BaseViewModel;

            T view = new T
            {
                DataContext = viewModel
            };

            if (viewModel.CloseAction == null)
                viewModel.CloseAction = new Action(view.Close);

            view.ShowDialog();
        }
    }
}
