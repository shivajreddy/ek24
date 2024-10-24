using System.Windows;

namespace ek24.Commands;

public partial class TestWindow : Window
{
    public TestWindow(TestViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

