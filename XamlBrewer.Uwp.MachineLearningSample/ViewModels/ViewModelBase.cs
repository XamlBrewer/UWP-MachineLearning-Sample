using System.Collections.ObjectModel;

namespace Mvvm
{
    internal class ViewModelBase : BindableBase
    {
        private static readonly ObservableCollection<MenuItem> AppMenu = new ObservableCollection<MenuItem>();
        private static readonly ObservableCollection<MenuItem> AppSecondMenu = new ObservableCollection<MenuItem>();

        public ViewModelBase()
        {}

        public ObservableCollection<MenuItem> Menu => AppMenu;

        public ObservableCollection<MenuItem> SecondMenu => AppSecondMenu;
    }
}
