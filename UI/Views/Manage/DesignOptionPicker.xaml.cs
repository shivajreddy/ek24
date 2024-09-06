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


namespace ek24.UI.Views.Manage;

public partial class DesignOptionPicker : Window
{
    public string SelectedDesignOption { get; private set; }

    public DesignOptionPicker(List<string> designOptions)
    {
        InitializeComponent();

        // Populate the ComboBox with design options
        DesignOptionComboBox.ItemsSource = designOptions;
        DesignOptionComboBox.SelectedIndex = 0; // Set the first item as selected by default
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // Set the selected design option
        SelectedDesignOption = DesignOptionComboBox.SelectedItem as string;
        CloseDialog(true);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CloseDialog(false);
    }

    private void CloseDialog(bool dialogResult)
    {
        // Find the parent window and close it
        Window parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            parentWindow.DialogResult = dialogResult;
            parentWindow.Close();
        }
    }
}
