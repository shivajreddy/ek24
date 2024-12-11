using ek24.UI.ViewModels.Properties;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace ek24.UI.Views.NewItem;


public partial class CreateBrandTypeView : UserControl
{
    public CreateBrandTypeView()
    {
        InitializeComponent();
        DataContext = new CreateBrandTypeViewModel();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine('h');
    }
}
