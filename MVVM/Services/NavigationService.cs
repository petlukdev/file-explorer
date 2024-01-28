using System.Collections.Generic;
using System.Linq;

namespace FileExplorer.MVVM.Services
{
    public static class NavigationService
    {
        private static int _currentPage = -1;
        private static List<string> _pages = new List<string>();
        public static bool CanGoBack { get { return _currentPage > 0; } }
        public static bool CanGoForward { get { return _currentPage < _pages.Count - 1; } }

        public static void AddPage(string page)
        {
            if (_currentPage < _pages.Count - 1)
                _pages.RemoveRange(_currentPage + 1, _pages.Count - _currentPage - 1);

            _pages.Add(page);
            _currentPage++;
        }

        public static string GetPreviousPage()
        {
            if (!CanGoBack) return _pages.First();

            _currentPage--;
            return _pages[_currentPage];
        }

        public static string GetNextPage()
        {
            if (!CanGoForward) return _pages.Last();

            _currentPage++;
            return _pages[_currentPage];
        }
    }
}
