using ek24.UI.ViewModels.ChangeBrand;
using System.Windows;


namespace ek24.UI.Views.ChangeBrand;


public partial class ChangeBrandView : Window
{

    public ChangeBrandView(ChangeBrandViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
