using System.Windows.Controls;

namespace ek24.UI;

public partial class EK24Modify_View : UserControl
{
    public EK24Modify_View()
    {
        InitializeComponent();
        DataContext = new EK24Modify_ViewModel();
        //DataContext = APP.global_state.eK24Modify_ViewModel;
        //DataContext = APP.global_state.current_project_state.eK24Modify_ViewModel;
    }

}

