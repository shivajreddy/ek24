using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ek24.Events;
using ek24.UI.ViewModels.Properties;


namespace ek24.UI.Views.Properties;


public partial class CurrentSelectionView : UserControl
{
    public CurrentSelectionView()
    {
        InitializeComponent();

        //UpdateFamilyAndTypeViewModel updateFamilyAndTypeViewModel = new UpdateFamilyAndTypeViewModel();
        //DataContext = updateFamilyAndTypeViewModel;

        DataContext = new CurrentSelectionViewModel();

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
    }
}
