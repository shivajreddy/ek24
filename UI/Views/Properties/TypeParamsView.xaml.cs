using ek24.UI.ViewModels.Properties;
using System.Windows.Controls;


namespace ek24.UI.Views.Properties;


public partial class TypeParamsView : UserControl
{
    public TypeParamsView()
    {
        //UpdateFamilyAndTypeViewModel updateFamilyAndTypeViewModel = new UpdateFamilyAndTypeViewModel();
        //DataContext = updateFamilyAndTypeViewModel;

        InitializeComponent();
        DataContext = new TypeParamsViewModel();

    }

}
