using ek24.UI.ViewModels.Properties;
using System.Windows.Controls;


namespace ek24.UI.Views.NewItem;


public partial class CreateFamilyTypeView : UserControl
{
    public CreateFamilyTypeView()
    {
        InitializeComponent();
        DataContext = new CreateFamilyTypeViewModel();
    }

    //private void Button_Click(object sender, RoutedEventArgs e)
    //{
    //    Debug.WriteLine("hi");
    //}
}
