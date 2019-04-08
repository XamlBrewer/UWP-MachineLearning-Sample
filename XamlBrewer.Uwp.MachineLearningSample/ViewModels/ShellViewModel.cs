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
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("RegressionIcon"), Text = "Regression", NavigationDestination = typeof(RegressionPage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("RecommendationIcon"), Text = "Recommendation", NavigationDestination = typeof(RecommendationPage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("FfmIcon"), Text = "FFM Recommendation", NavigationDestination = typeof(FieldAwareFactorizationPage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("BoxPlotIcon"), Text = "Feature Distribution", NavigationDestination = typeof(BoxPlotPage) });
            Menu.Add(new MenuItem() { Glyph = Icon.GetIcon("HeatMapIcon"), Text = "Feature Correlation", NavigationDestination = typeof(HeatMapPage) });
            SecondMenu.Add(new MenuItem() { Glyph = Icon.GetIcon("InfoIcon"), Text = "About", NavigationDestination = typeof(AboutPage) });
        }
    }
}
