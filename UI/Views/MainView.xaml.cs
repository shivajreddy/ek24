using Autodesk.Revit.UI;
using ek24.UI.ViewModels;


namespace ek24.UI.Views;


// Main UI that implements IDockablePaneProvider that is
// required by revit to show wpf
public partial class MainView : IDockablePaneProvider
{
    public MainView()
    {
        InitializeComponent();

        DataContext = new MainViewModel();
    }

    // Required to be implemented by the IDockablePaneProvider
    public void SetupDockablePane(DockablePaneProviderData data)
    {
        data.FrameworkElement = this;
        data.InitialState = new DockablePaneState
        {
            DockPosition = DockPosition.Tabbed,
            TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
        };
    }
}
