using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Mvvm.Services
{
    public static class Navigation
    {
        private static Frame _frame;
        private static readonly EventHandler<BackRequestedEventArgs> _goBackHandler = (s, e) => Navigation.GoBack();

        public static Frame Frame
        {
            get { return _frame; }
            set { _frame = value; }
        }

        public static bool Navigate(Type sourcePageType, object parameter = null)
        {
            if (_frame.CurrentSourcePageType == sourcePageType)
            {
                return true;
            }

            return _frame.Navigate(sourcePageType, parameter);
        }

        public static void EnableBackButton()
        {
            var navManager = SystemNavigationManager.GetForCurrentView();
            navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            navManager.BackRequested -= _goBackHandler;
            navManager.BackRequested += _goBackHandler;
        }

        public static void DisableBackButton()
        {
            var navManager = SystemNavigationManager.GetForCurrentView();
            navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            navManager.BackRequested -= _goBackHandler;
        }

        public static void GoBack()
        {
            if (_frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }
    }
}
