using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using FileExplorer.MVVM.Helpers;
using FileExplorer.MVVM.Models;
using Newtonsoft.Json;

namespace FileExplorer.MVVM.Fetchers
{
    public static class FavoritesFetcher
    {
        private static int _defaultRow = 7;
        private static string[,] _defaultFavorites = {
            { "My Computer", "My Computer" },
            { "Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
            { "Downloads", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") },
            { "Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
            { "Videos", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) },
            { "Music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) },
            { "Pictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) }
        };

        private static string _favoritesStoreDirPath;
        private static string _favoritesStoreFilePath;

        public static List<TreeViewItem> FavoritesTreeCollection = new List<TreeViewItem>();
        public static List<FavoriteModel> FavoritesModelCollection = new List<FavoriteModel>();

        public static void SetupFavorites()
        {
            string fileName =  $"favorites_{Environment.UserName}.json";
            _favoritesStoreDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileExplorer_Lukas");
            _favoritesStoreFilePath = Path.Combine(_favoritesStoreDirPath, fileName);

            StoreDirExists();
            StoreFileExists();

            foreach (FavoriteModel item in FavoritesModelCollection)
                FavoritesTreeCollection.Add(CreateFavoriteTreeItem(item.Name, item.Path));
        }

        public static void StoreCollection()
        {
            string json = JsonConvert.SerializeObject(FavoritesModelCollection);
            File.WriteAllText(_favoritesStoreFilePath, json);
        }

        public static void AddToCollections(TreeViewItem item)
        {
            FavoritesTreeCollection.Add(item);
            FavoritesModelCollection.Add(CreateFavoriteItem((string)item.Header, (string)item.Tag));
        }

        public static void RemoveFromCollections(FavoriteModel item)
        {
            FavoritesModelCollection.Remove(item);
            FavoritesTreeCollection.RemoveAll(a => (string)a.Header == item.Name || (string)a.Tag == item.Path);
        }

        private static void StoreDirExists()
        {
            if (!_favoritesStoreDirPath.IsDirectory())
            {
                Directory.CreateDirectory(_favoritesStoreDirPath);
                File.Create(_favoritesStoreFilePath).Dispose();
            }
        }

        private static void StoreFileExists()
        {
            if (!_favoritesStoreFilePath.IsFile())
            {
                for (int i = 0; i < _defaultRow; i++)
                    FavoritesModelCollection.Add(CreateFavoriteItem(_defaultFavorites[i, 0], _defaultFavorites[i, 1]));

                File.Create(_favoritesStoreFilePath).Dispose();
                string json = JsonConvert.SerializeObject(FavoritesModelCollection);
                File.WriteAllText(_favoritesStoreFilePath, json);
            }
            else if (new FileInfo(_favoritesStoreFilePath).Length <= 0)
            {
                for (int i = 0; i < _defaultRow; i++)
                    FavoritesModelCollection.Add(CreateFavoriteItem(_defaultFavorites[i, 0], _defaultFavorites[i, 1]));

                string json = JsonConvert.SerializeObject(FavoritesModelCollection);
                File.WriteAllText(_favoritesStoreFilePath, json);
            }
            else
            {
                string json = File.ReadAllText(_favoritesStoreFilePath);
                FavoritesModelCollection = JsonConvert.DeserializeObject<List<FavoriteModel>>(json);
            }
        }

        private static FavoriteModel CreateFavoriteItem(string name, string path)
        {
            FavoriteModel item = new FavoriteModel
            {
                Name = name,
                Path = path
            };

            return item;
        }
        private static TreeViewItem CreateFavoriteTreeItem(string name, string path)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = name,
                Tag = path
            };

            return item;
        }
    }
}
