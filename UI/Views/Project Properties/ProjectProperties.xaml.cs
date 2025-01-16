using System.Windows;
using System.Windows.Controls;


namespace ek24.UI;



public partial class EK24ProjectProperties_View : UserControl
{
    public EK24ProjectProperties_View()
    {
        InitializeComponent();
        //var viewModel2 = new ChangeBrandViewModel(commandData.Application);
        //var viewModel = new EK24ProjectProperties_ViewModel();
        //DataContext = new InstanceParamsViewModel();
        DataContext = new EK24ProjectProperties_ViewModel();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
