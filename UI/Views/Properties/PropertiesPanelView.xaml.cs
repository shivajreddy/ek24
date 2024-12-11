using ek24.UI.ViewModels.Properties;
using System.Windows.Controls;


namespace ek24.UI.Views.Properties;


public partial class PropertiesViewView : UserControl
{
    public PropertiesViewView()
    {
        InitializeComponent();
        DataContext = new PropertiesPanelViewModel();
    }
}
