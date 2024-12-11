using ek24.UI.ViewModels.NewItem;
using System.Windows.Controls;

namespace ek24.UI.Views.NewItem;

public partial class NewItemView : UserControl
{
    public NewItemView()
    {
        InitializeComponent();
        DataContext = new NewItemViewModel();
    }
}
