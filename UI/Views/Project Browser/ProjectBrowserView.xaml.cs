using System.Diagnostics;
using System.Windows.Controls;

using ek24.UI.ViewModels.ProjectBrowser;


namespace ek24.UI.Views.ProjectBrowser;


public partial class ProjectBrowserView : UserControl
{
    public ProjectBrowserView()
    {
        InitializeComponent();

        /// set the DataContext to the ViewModel of this View
        DataContext = new ProjectBrowserViewModel();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

        Debug.WriteLine("button clicked");
    }
}
