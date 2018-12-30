using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mvvm.Services
{
    public static class ActivationExtensions
    {
        public static Window EnsureRootFrame(this Window window)
        {
            var frame = window.Content as Frame;
            if (frame == null)
            {
                frame = new Frame();
                frame.NavigationFailed += (sender, e) => { throw new Exception("Failed to load Page " + e.SourcePageType.FullName); };
                window.Content = frame;
            }

            return window;
        }

        public static Window NavigateIfAppropriate(this Window window, Type sourcePageType, object parameter)
        {
            var frame = window.Content as Frame;

            if (frame == null)
            {
                return window;
            }

            if (frame.Content == null)
            {
                frame.Navigate(sourcePageType, parameter);
            }

            return window;
        }
    }
}
