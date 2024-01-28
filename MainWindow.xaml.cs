using FileExplorer.MVVM.Fetchers;
using System;
using System.Windows;


namespace FileExplorer
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e) => FavoritesFetcher.StoreCollection();
    }
}
