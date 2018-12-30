using System;
using System.Collections.Generic;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Mvvm.Services
{
    public class Theme
    {
        private static Dictionary<Type, Color> AccentColors = new Dictionary<Type, Color>();

        private static Color DefaultAccentColor => (Color)Application.Current.Resources["DefaultAccentColor"];

        private static Color TitleBarButtonForegroundColor => (Color)Application.Current.Resources["TitlebarButtonForegroundColor"];

        // Call this in App OnLaunched.
        // Requires reference to Windows Mobile Extensions for the UWP.
        /// <summary>
        /// Applies to the theme to the Application View.
        /// </summary>
        public static void ApplyToContainer()
        {
            // Custom accent color.
            ApplyAccentColor(DefaultAccentColor);

            // Title bar - if present.
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                    titleBar.ButtonForegroundColor = TitleBarButtonForegroundColor;
                    titleBar.ButtonHoverBackgroundColor = TitleBarButtonForegroundColor;
                    titleBar.ButtonPressedBackgroundColor = TitleBarButtonForegroundColor;
                }
            }
        }

        /// <summary>
        /// Applies the accent color.
        /// </summary>
        /// <param name="accentColor">Desired accent color.</param>
        /// <remarks>Ignored when user selected high contrast mode.</remarks>
        public static void ApplyAccentColor(Color accentColor)
        {
            if (!new AccessibilitySettings().HighContrast)
            {
                Application.Current.Resources["SystemAccentColor"] = accentColor;
            }
        }

        public static void ApplyAccentColor(Type pageType)
        {
            if (AccentColors.ContainsKey(pageType))
            {
                ApplyAccentColor(AccentColors[pageType]);
            }
            else
            {
                ApplyAccentColor((Color)Application.Current.Resources["DefaultAccentColor"]);
            }
        }

        public static void RegisterAccentColor(Type pageType, Color accentColor)
        {
            if (AccentColors.ContainsKey(pageType))
            {
                AccentColors.Add(pageType, accentColor);
            }
            else
            {
                AccentColors[pageType] = accentColor;
            }
        }
    }
}