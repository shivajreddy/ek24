using System.Diagnostics;
using System.Windows.Controls;

using ek24.UI.ViewModels;


namespace ek24.UI.Views;


public partial class ProjectBrowser : UserControl
{
    public ProjectBrowser()
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
