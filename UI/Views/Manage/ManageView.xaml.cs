using System.Windows.Controls;

using ek24.UI.ViewModels.Manage;


namespace ek24.UI.Views.Manage;


public partial class ManageView : UserControl
{
    public ManageView()
    {
        InitializeComponent();
        DataContext = new ManageViewModel();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}
