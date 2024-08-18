using System.Windows.Controls;

using ek24.UI.ViewModels.ProjectBrowser;


namespace ek24.UI.Views.ProjectBrowser;


public partial class SelectCaseWorkView : UserControl
{
    public SelectCaseWorkView()
    {
        InitializeComponent();
        DataContext = new SelectCaseWorkViewModel();
    }
}
