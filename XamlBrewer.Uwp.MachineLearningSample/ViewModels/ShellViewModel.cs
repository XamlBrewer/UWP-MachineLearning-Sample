using Mvvm.Services;
using XamlBrewer.Uwp.MachineLearningSample;

namespace Mvvm
{
    internal class ShellViewModel : ViewModelBase
    {
        public ShellViewModel()
        {
            // Build the menus
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("HomeIcon"), Text = "Home", NavigationDestination = typeof(HomePage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("ClusteringIcon"), Text = "Clustering", NavigationDestination = typeof(ClusteringPage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("ClassificationIcon"), Text = "Multi Classification", NavigationDestination = typeof(ClassificationPage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("BinaryClassificationIcon"), Text = "Binary Classification", NavigationDestination = typeof(BinaryClassificationPage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("BoxPlotIcon"), Text = "Feature Analysis", NavigationDestination = typeof(BoxPlotPage) });
            SecondMenu.Add(new MenuItem() { Glyph = Icon.GetIcon("InfoIcon"), Text = "About", NavigationDestination = typeof(AboutPage) });
        }
    }
}
